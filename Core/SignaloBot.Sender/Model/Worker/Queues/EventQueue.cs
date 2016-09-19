using Common.Utility;
using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue
{
    public class EventQueue<TKey> : QueueBase<SignalEventBase<TKey>, TKey>, IEventQueue<TKey>
        where TKey : struct
    {
        //свойства
        public virtual ISignalEventQueries<TKey> Queries { get; set; }



        //инициализация
        public EventQueue(ISignalEventQueries<TKey> queries)
            : base()
        {
            Queries = queries;
        }



        //tick
        public virtual void OnTick(IStatisticsCollector<TKey> stats)
        {
            bool isQueryRequired = CheckIsQueryRequired();
            if (isQueryRequired)
            {
                QueryStorage(stats);
            }

            ReturnExtraItems();

            bool isFlushTime = CheckIsFlushRequired();
            if (isFlushTime)
            {
                FlushResults();
            }
        }


        //Dequeue and apply result
        public virtual SignalWrapper<SignalEventBase<TKey>> DequeueNext()
        {
            SignalWrapper<SignalEventBase<TKey>> item = null;

            lock (_queueLock)
            {
                foreach (KeyValuePair<int, Queue<SignalWrapper<SignalEventBase<TKey>>>> group in _itemsQueue)
                {
                    if (group.Value.Count > 0)
                    {
                        item = group.Value.Dequeue();
                        break;
                    }
                }
            }
            
            if (item != null && CountQueueItems() == 0)
            {
                _firstQueryAfterEmptyRequired = true;
            }

            return item;
        }
        public override void ApplyResult(
            SignalWrapper<SignalEventBase<TKey>> item, ProcessingResult result)
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
                Append(item);                
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
        

        //Query and append to queue
        public virtual bool CheckIsQueryRequired()
        {
            bool isEmpty = CountQueueItems() == 0;

            bool storageUpdated = InsertNotifier != null
                && InsertNotifier.HasUpdates;

            DateTime nextQueryTimeUtc = _lastQueryTimeUtc + QueryPeriod;
            bool doScheduledQuery = nextQueryTimeUtc <= DateTime.UtcNow;

            bool queryAfterMaxItemsReceived = _isMaxItemsReceived && _firstQueryAfterEmptyRequired;

            return isEmpty &&
                (storageUpdated || doScheduledQuery || queryAfterMaxItemsReceived);
        }
        protected virtual void QueryStorage(IStatisticsCollector<TKey> stats)
        {
            _lastQueryTimeUtc = DateTime.UtcNow;
            _firstQueryAfterEmptyRequired = false;
            _isMaxItemsReceived = false;
            if (InsertNotifier != null)
            {
                InsertNotifier.HasUpdates = false;                
            }

            Stopwatch storageQueryTimer = Stopwatch.StartNew();

            QueryResult<List<SignalEventBase<TKey>>> items =
                Queries.Select(ItemsQueryCount, MaxFailedAttempts).Result;

            if (stats != null)
            {
                stats.EventStorageQueried(storageQueryTimer.Elapsed, items);
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
        public virtual void Append(List<SignalEventBase<TKey>> items, bool isStored)
        {
            List<IGrouping<int, SignalWrapper<SignalEventBase<TKey>>>> groups = items
                .Select(p => new SignalWrapper<SignalEventBase<TKey>>(p, isStored))
                .GroupBy(p => p.Signal.CategoryID)
                .ToList();

            Append(groups);
        }
        public virtual void Append(SignalWrapper<SignalEventBase<TKey>> item)
        {
            Append(item, item.Signal.CategoryID);
        }


        //Flush results
        protected override void FlushResults()
        {
            _flushQueue[FlushAction.Insert].Flush((list) => Queries.Insert(list));
            _flushQueue[FlushAction.Delete].Flush((list) => Queries.Delete(list));
            _flushQueue[FlushAction.Update].Flush((list) => Queries.Update(list));
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
