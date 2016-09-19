using Common.Utility;
using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue
{
    public abstract class QueueBase<TS, TKey>
        where TKey : struct
    {
        //поля
        protected object _queueLock;
        protected bool _isMaxItemsReceived;
        protected bool _firstQueryAfterEmptyRequired;
        protected DateTime _lastQueryTimeUtc;
        protected Dictionary<int, Queue<SignalWrapper<TS>>> _itemsQueue;

        protected DateTime _lastFlushTimeUtc;
        protected Dictionary<FlushAction, FlushQueue<TS>> _flushQueue;
        

        //свойства
        public virtual IInsertNotifier InsertNotifier { get; set; }
                
        /// <summary>
        /// Период обращения к хранилищу для поиска новых сообщений в очереди и отправки отложенных сообщений.
        /// </summary>
        public virtual TimeSpan QueryPeriod { get; set; }        

        /// <summary>
        /// Количество сообщений получаемых из хранилища за 1 запрос.
        /// </summary>
        public virtual int ItemsQueryCount { get; set; }
        
        /// <summary>
        /// Максимальное количество попыток 
        /// </summary>
        public virtual int MaxFailedAttempts { get; set; }
        
        public virtual TimeSpan FlushPeriod { get; set; }

        public virtual int ReturnToStorageAfterItemsCount { get; set; }
        



        //инициализация
        public QueueBase()
        {
            _itemsQueue = new Dictionary<int, Queue<SignalWrapper<TS>>>();
            _queueLock = new object();

            _flushQueue = new Dictionary<FlushAction, FlushQueue<TS>>()
            {
                { FlushAction.Insert, new FlushQueue<TS>() },
                { FlushAction.Delete, new FlushQueue<TS>() },
                { FlushAction.Skip, new FlushQueue<TS>() },
                { FlushAction.Update, new FlushQueue<TS>() }
            };
            
            QueryPeriod = SenderConstants.QUEUE_QUERY_PERIOD;
            ItemsQueryCount = SenderConstants.QUEUE_ITEMS_QUERY_COUNT;
            MaxFailedAttempts = SenderConstants.QUEUE_MAX_FAILED_ATTEMPTS;
            FlushPeriod = SenderConstants.QUEUE_FLUSH_PERIOD;
            ReturnToStorageAfterItemsCount = SenderConstants.QUEUE_RETURN_TO_STORAGE_AFTER_ITEMS_COUNT;
        }


        
        //Query and append to queue 
        protected virtual void Append(List<IGrouping<int, SignalWrapper<TS>>> groups)
        {
            lock (_queueLock)
            {
                foreach (IGrouping<int, SignalWrapper<TS>> group in groups)
                {
                    if (!_itemsQueue.ContainsKey(group.Key))
                    {
                        _itemsQueue.Add(group.Key, new Queue<SignalWrapper<TS>>());
                    }

                    foreach (SignalWrapper<TS> item in group)
                    {
                        _itemsQueue[group.Key].Enqueue(item);
                    }
                }
            }
        }
        public virtual void Append(SignalWrapper<TS> item, int key)
        {
            lock (_queueLock)
            {
                if (!_itemsQueue.ContainsKey(key))
                {
                    _itemsQueue.Add(key, new Queue<SignalWrapper<TS>>());
                }

                _itemsQueue[key].Enqueue(item);
            }
        }
        

        //Dequeue and apply result
        public virtual int CountQueueItems()
        {
            lock (_queueLock)
            {
                return _itemsQueue.Sum(p => p.Value.Count);
            }
        }
        public virtual bool CheckIsEmpty(List<int> activeKeys)
        {
            lock (_queueLock)
            {
                return !_itemsQueue.Any(p => activeKeys.Contains(p.Key) && p.Value.Count > 0);
            }
        }                
        public abstract void ApplyResult(SignalWrapper<TS> item, ProcessingResult result);

        
        //Flush results
        protected virtual bool CheckIsFlushRequired()
        {
            DateTime nextFlushTimeUtc = _lastFlushTimeUtc + FlushPeriod;
            bool doScheduledQuery = nextFlushTimeUtc <= DateTime.UtcNow;

            bool hasItems = _flushQueue.Any(p => p.Value.Queue.Count > 0);

            return doScheduledQuery && hasItems;
        }
        protected abstract void FlushResults();


        //Return extra
        internal virtual void ReturnExtraItems(List<int> activeKeys = null)
        {
            lock (_queueLock)
            {
                int currentItemsCount = _itemsQueue.Sum(p => p.Value.Count);
                bool isLimitExceeded = currentItemsCount > ReturnToStorageAfterItemsCount;

                int targetItemsCount = ItemsQueryCount;
                int extraItems = currentItemsCount - targetItemsCount;

                if (isLimitExceeded && extraItems > 0)
                {
                    if(activeKeys == null)
                    {
                        activeKeys = _itemsQueue.Select(p => p.Key).ToList();
                    }

                    List<int> notActiveKeys = _itemsQueue
                        .Where(p => !activeKeys.Contains(p.Key))
                        .Select(p => p.Key)
                        .ToList();

                    ReturnExtraItems(notActiveKeys, ref extraItems);
                    ReturnExtraItems(activeKeys, ref extraItems);
                }
            }
        }
        private void ReturnExtraItems(List<int> keys, ref int extraItems)
        {
            List<KeyValuePair<int, Queue<SignalWrapper<TS>>>> queues = _itemsQueue
                .Where(p => keys.Contains(p.Key) && p.Value.Count > 0)
                .ToList();

            foreach (KeyValuePair<int, Queue<SignalWrapper<TS>>> queue in queues)
            {
                if (extraItems == 0)
                {
                    break;
                }

                List<SignalWrapper<TS>> list = queue.Value.ToList();
                int itemsRemoved = 0;

                for (int i = list.Count - 1;
                    i >= 0 && itemsRemoved < extraItems;
                    i--, itemsRemoved++)
                {
                    ApplyResult(list[i], ProcessingResult.Return);
                }

                int itemsLeftCount = list.Count - itemsRemoved;
                list.RemoveRange(itemsLeftCount, itemsRemoved);
                _itemsQueue[queue.Key] = new Queue<SignalWrapper<TS>>(list);

                extraItems -= itemsRemoved;
            }
        }        
        public virtual void ReturnAll()
        {
            lock (_queueLock)
            {
                foreach (KeyValuePair<int, Queue<SignalWrapper<TS>>> queue in _itemsQueue)
                {
                    while (queue.Value.Count > 0)
                    {
                        SignalWrapper<TS> next = queue.Value.Dequeue();
                        ApplyResult(next, ProcessingResult.Return);
                    }
                }
            }

            FlushResults();
        }

    }
}
