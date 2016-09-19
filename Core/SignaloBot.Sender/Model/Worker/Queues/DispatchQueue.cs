using SignaloBot.DAL;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq.Expressions;
using SignaloBot.Sender;
using SignaloBot.Sender.Statistics;
using SignaloBot.Sender.Processors;

namespace SignaloBot.Sender.Queue
{
    public class DispatchQueue<TKey>
        : QueueBase<SignalDispatchBase<TKey>, TKey>, IDispatchQueue<TKey>
        where TKey : struct
    {
        //поля
        protected List<int> _lastQueryKeys;


        //свойства
        public virtual ISignalDispatchQueueQueries<TKey> Queries { get; set; }
        /// <summary>
        /// Период повторной отправки сообщения после неуспешной попытки.
        /// </summary>
        public virtual TimeSpan FailedAttemptRetryPeriod { get; set; }


        //инициализация
        public DispatchQueue(ISignalDispatchQueueQueries<TKey> queries)
            : base()
        {
            Queries = queries;

            ReturnToStorageAfterItemsCount = int.MaxValue;
            FailedAttemptRetryPeriod = SenderConstants.QUEUE_FAILED_ATTEMPT_RETRY_PERIOD;
        }


        //tick
        public virtual void OnTick(List<int> activeKeys, IStatisticsCollector<TKey> stats)
        {
            bool isQueryRequired = CheckIsQueryRequired(activeKeys);
            if (isQueryRequired)
            {
                QueryStorage(activeKeys, stats);
            }

            ReturnExtraItems(activeKeys);

            bool isFlushTime = CheckIsFlushRequired();
            if (isFlushTime)
            {
                FlushResults();
            }
        }



        //Query and append to queue  
        public virtual bool CheckIsQueryRequired(List<int> deliveryTypes)
        {
            bool isEmpty = CheckIsEmpty(deliveryTypes);

            bool storageUpdated = InsertNotifier != null
                && InsertNotifier.HasUpdates;

            DateTime nextQueryTimeUtc = _lastQueryTimeUtc + QueryPeriod;
            bool doScheduledQuery = nextQueryTimeUtc <= DateTime.UtcNow;

            bool hasUnqueriedKeys = _lastQueryKeys == null
               || deliveryTypes.Except(_lastQueryKeys).Count() > 0;

            bool queryAfterMaxItemsReceived = _isMaxItemsReceived && _firstQueryAfterEmptyRequired;

            return isEmpty &&
                (storageUpdated || doScheduledQuery || hasUnqueriedKeys || queryAfterMaxItemsReceived);
        }
        protected virtual void QueryStorage(
            List<int> deliveryTypes, IStatisticsCollector<TKey> stats)
        {
            _lastQueryTimeUtc = DateTime.UtcNow;
            _lastQueryKeys = deliveryTypes;
            _firstQueryAfterEmptyRequired = false;
            _isMaxItemsReceived = false;
            if (InsertNotifier != null)
            {
                InsertNotifier.HasUpdates = false;                
            }

            Stopwatch storageQueryTimer = Stopwatch.StartNew();
          
            QueryResult<List<SignalDispatchBase<TKey>>> items =
                Queries.Select(ItemsQueryCount, deliveryTypes, MaxFailedAttempts).Result;
            
            if (stats != null)
            {
                stats.DispatchStorageQueried(storageQueryTimer.Elapsed, items);
            }

            if (items.HasExceptions)
            {
                return;
            }

            if (InsertNotifier != null && items.Result.Count == 0)
            {
                InsertNotifier.StartMonitor();
            }

            _isMaxItemsReceived = items.Result.Count == ItemsQueryCount;
            Append(items.Result, true);
        }
        public virtual void Append(List<SignalDispatchBase<TKey>> items, bool isStored)
        {
            lock (_queueLock)
            {
                foreach (SignalDispatchBase<TKey> item in items)
                {
                    var signal = new SignalWrapper<SignalDispatchBase<TKey>>(item, isStored);

                    if (item.IsDelayed && item.SendDateUtc > DateTime.UtcNow)
                    {
                        ApplyResult(signal, ProcessingResult.Return);
                        return;
                    }

                    if (!_itemsQueue.ContainsKey(item.DeliveryType))
                    {
                        _itemsQueue.Add(item.DeliveryType, new Queue<SignalWrapper<SignalDispatchBase<TKey>>>());
                    }

                    _itemsQueue[item.DeliveryType].Enqueue(signal);
                }
            }
        }
     
        

        //Dequeue and apply result
        public virtual SignalWrapper<SignalDispatchBase<TKey>> DequeueNext(
            List<int> deliveryTypes)
        {
            SignalWrapper<SignalDispatchBase<TKey>> item = null;

            lock (_queueLock)
            {
                foreach (KeyValuePair<int, Queue<SignalWrapper<SignalDispatchBase<TKey>>>> group in _itemsQueue)
                {
                    if (deliveryTypes.Contains(group.Key) && group.Value.Count > 0)
                    {
                        item = group.Value.Dequeue();
                        break;
                    }
                }
            }
            
            if (item != null && CheckIsEmpty(deliveryTypes))
            {
                _firstQueryAfterEmptyRequired = true;
            }

            return item;
        }
        public override void ApplyResult(
            SignalWrapper<SignalDispatchBase<TKey>> item, ProcessingResult result)
        {
            if (result == ProcessingResult.Success)
            {
                if (item.IsStored)
                {
                    _flushQueue[FlushAction.Delete].Queue.Add(item);
                }
            }
            else if (result == ProcessingResult.Fail)
            {
                item.Signal.FailedAttempts++;
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(FailedAttemptRetryPeriod);
                item.Signal.IsDelayed = false;

                if (item.IsStored)
                {
                    _flushQueue[FlushAction.Update].Queue.Add(item);
                }
                else
                {
                    _flushQueue[FlushAction.Insert].Queue.Add(item);
                }
            }
            else if (result == ProcessingResult.Repeat)
            {
                Append(item, item.Signal.DeliveryType);
            }
            else if (result == ProcessingResult.NoHandlerFound)
            {
                if (item.IsStored)
                {
                    _flushQueue[FlushAction.Delete].Queue.Add(item);
                }
            }
            else if (result == ProcessingResult.Return)
            {
                if (!item.IsStored)
                {
                    _flushQueue[FlushAction.Insert].Queue.Add(item);
                }
                else if (item.IsUpdated)
                {
                    _flushQueue[FlushAction.Update].Queue.Add(item);
                }
            }
        }


        //Flush results
        protected override void FlushResults()
        {
            _flushQueue[FlushAction.Insert].Flush((list) => Queries.Insert(list));
            _flushQueue[FlushAction.Delete].Flush((list) => Queries.Delete(list));
            _flushQueue[FlushAction.Update].Flush((list) => Queries.UpdateFailedAttempts(list));
            _flushQueue[FlushAction.Skip].Flush((list) => Task.FromResult(true));

            Task[] flushTasks = _flushQueue
                .Select(p => p.Value.FlushTask)
                .ToArray();
            Task.WaitAll(flushTasks);

            _flushQueue[FlushAction.Insert].Remove();
            _flushQueue[FlushAction.Delete].Remove();
            _flushQueue[FlushAction.Update].Remove();
            _flushQueue[FlushAction.Skip].Remove();
        }
        

        //IDisposable
        public virtual void Dispose()
        {
            if (Queries != null)
                Queries.Dispose();

            if (InsertNotifier != null)
                InsertNotifier.Dispose();
        }
    }
}
