using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing
{
    public abstract class FlushJobBase<T> : IRegularJob
    {
        //fields
        protected DateTime _lastFlushTimeUtc;
        protected Dictionary<FlushAction, FlushQueue<T>> _flushQueues;
        protected List<Action<List<T>>> _flushedItemsHandlers = new List<Action<List<T>>>();


        //properties
        /// <summary>
        /// Period between batch queries to flush processing results to permanent storage.
        /// </summary>
        public TimeSpan FlushPeriod { get; set; }
        /// <summary>
        /// Limit of queue size in FlushJob when exceeded starts to flush processing results to permanent storage.
        /// </summary>
        public int QueueLimit { get; set; }
        /// <summary>
        /// Wait during FlushJobFlushPeriod to accumulate batch of notifications to flush into database.
        /// </summary>
        public bool IsBatchingEnabled { get; set; }


        //init
        public FlushJobBase(SenderSettings senderSettings)
        {
            FlushPeriod = senderSettings.FlushJobFlushPeriod;
            QueueLimit = senderSettings.FlushJobQueueLimit;
            IsBatchingEnabled = senderSettings.FlushJobBatchingEnabled;

            //how to flush items
            _flushQueues = new Dictionary<FlushAction, FlushQueue<T>>();
        }


        //IRegularJob methods
        public virtual void Tick()
        {
            bool isFlushRequired = CheckIsFlushRequired();
            if (isFlushRequired)
            {
                FlushQueues();
            }
        }

        public virtual void Flush()
        {
            FlushQueues();
        }


        //perform flush
        protected virtual bool CheckIsFlushRequired()
        {
            bool hasItems = _flushQueues.Any(p => p.Value.Queue.Count > 0);
            if (!hasItems)
            {
                return false;
            }

            DateTime nextFlushTimeUtc = _lastFlushTimeUtc + FlushPeriod;
            bool doScheduledQuery = nextFlushTimeUtc <= DateTime.UtcNow;

            bool hasMaxItems = _flushQueues.Sum(p => p.Value.Queue.Count) > QueueLimit;

            return doScheduledQuery || hasMaxItems;
        }

        protected virtual void FlushQueues()
        {
            _lastFlushTimeUtc = DateTime.UtcNow;

            //make database queries and wait for completion
            Task[] flushTasks = _flushQueues.Values
                .Select(flushQueue => flushQueue.Flush())
                .ToArray();
            Task.WaitAll(flushTasks);

            //remove items from queue
            List<T> flushedItems = _flushQueues.Values
                .SelectMany(x => x.Clear())
                .ToList();

            foreach (Action<List<T>> handlers in _flushedItemsHandlers)
            {
                handlers.Invoke(flushedItems);
            }
        }


        //enqueue item
        public virtual void EnqueueItem(T item, FlushAction action)
        {
            _flushQueues[action].Queue.Add(item);

            if (!IsBatchingEnabled)
            {
                FlushQueues();
            }
        }
    }
}
