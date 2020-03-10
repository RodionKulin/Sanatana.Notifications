using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing
{
    public abstract class SignalFlushJobBase<TSignal> : IRegularJob, ISignalFlushJob<TSignal>
    {
        //fields
        protected DateTime _lastFlushTimeUtc;
        protected Dictionary<int, Queue<SignalWrapper<TSignal>>> _itemsQueue;
        protected Dictionary<FlushAction, FlushQueue<TSignal>> _flushQueues;
        protected ISignalQueries<TSignal> _queries;
        protected ITemporaryStorage<TSignal> _temporaryStorage;
        protected TemporaryStorageParameters _temporaryStorageParameters;


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
        /// Enable removing items from temporary storage while they are successfully flushed to permanent storage.
        /// </summary>
        public bool IsTemporaryStorageEnabled { get; set; }


        //init
        public SignalFlushJobBase(ITemporaryStorage<TSignal> temporaryStorage, ISignalQueries<TSignal> queries)
        {
            _temporaryStorage = temporaryStorage;
            _queries = queries;

            _flushQueues = new Dictionary<FlushAction, FlushQueue<TSignal>>()
            {
                { FlushAction.Insert, new FlushQueue<TSignal>() },
                { FlushAction.Delete, new FlushQueue<TSignal>() },
                { FlushAction.Update, new FlushQueue<TSignal>() }
            };
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


        //other methods
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

            _flushQueues[FlushAction.Insert].Flush((list) => _queries.Insert(list));
            _flushQueues[FlushAction.Update].Flush((list) => _queries.UpdateSendResults(list));
            _flushQueues[FlushAction.Delete].Flush((list) => _queries.Delete(list));

            Task[] flushTasks = _flushQueues
                .Select(p => p.Value.FlushTask)
                .ToArray();
            Task.WaitAll(flushTasks);

            //dequeue items from flush queue and get items Ids that were successfully flushed to remove them from temp storage.
            List<Guid> tempStorageIds = _flushQueues
                .SelectMany(x => x.Value.Clear())
                .Where(x => x.TempStorageId.HasValue)
                .Select(x => x.TempStorageId.Value)
                .ToList();

            if (IsTemporaryStorageEnabled)
            {
                _temporaryStorage.Delete(_temporaryStorageParameters, tempStorageIds);
            }
        }

        public virtual void Delete(SignalWrapper<TSignal> item)
        {
            if (IsTemporaryStorageEnabled && item.TempStorageId != null)
            {
                _temporaryStorage.Delete(_temporaryStorageParameters, item.TempStorageId.Value);
                item.TempStorageId = null;
            }

            if (item.IsPermanentlyStored)
            {
                _flushQueues[FlushAction.Delete].Queue.Add(item);
            }
        }

        public virtual void Return(SignalWrapper<TSignal> item)
        {
            if (item.IsPermanentlyStored == false)
            {
                //Temp storage item will be deleted after flushing to permanent storage
                _flushQueues[FlushAction.Insert].Queue.Add(item);
            }
            else if (item.IsPermanentlyStored == true && item.IsUpdated)
            {
                if (IsTemporaryStorageEnabled && item.TempStorageId != null)
                {
                    _temporaryStorage.Update(_temporaryStorageParameters, item.TempStorageId.Value, item.Signal);
                }

                _flushQueues[FlushAction.Update].Queue.Add(item);
            }
        }


    }
}
