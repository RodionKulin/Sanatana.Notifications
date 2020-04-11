using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Sender;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Flushing.Queues;

namespace Sanatana.Notifications.Queues
{
    public class DispatchQueue<TKey> : QueueBase<SignalDispatch<TKey>, TKey>
        , IDispatchQueue<TKey>
        where TKey : struct
    {
        //fields
        protected IDispatchChannelRegistry<TKey> _dispatcherRegistry;
        protected ISignalFlushJob<SignalDispatch<TKey>> _signalFlushJob;
        protected ILogger _logger;


        //properties
        /// <summary>
        /// Pause duration after failed attempt or dispatcher not available before retrying.
        /// </summary>
        public TimeSpan RetryPeriod { get; set; }


        //init
        public DispatchQueue(SenderSettings senderSettings, ITemporaryStorage<SignalDispatch<TKey>> temporaryStorage
            , IDispatchChannelRegistry<TKey> dispatcherRegistry, ISignalFlushJob<SignalDispatch<TKey>> signalFlushJob 
            , ILogger logger)
            : base(temporaryStorage)
        {
            _dispatcherRegistry = dispatcherRegistry;
            _signalFlushJob = signalFlushJob;
            _logger = logger;

            RetryPeriod = senderSettings.SignalQueueRetryPeriod;
            PersistBeginOnItemsCount = senderSettings.SignalQueuePersistBeginOnItemsCount;
            PersistEndOnItemsCount = senderSettings.SignalQueuePersistEndOnItemsCount;
            IsTemporaryStorageEnabled = senderSettings.SignalQueueIsTemporaryStorageEnabled;

            _temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_DISPATCH_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };
        }


        //regular job methods
        public virtual void Tick()
        {
            List<int> activeDeliveryTypes = _dispatcherRegistry.GetActiveDeliveryTypes(false);
            ReturnExtraItems(activeDeliveryTypes);
        }



        //queue methods
        public virtual void Append(List<SignalDispatch<TKey>> items, bool isStored)
        {
            IEnumerable<SignalDispatch<TKey>> scheduledItems =
                items.Where(x => x.IsScheduled && x.SendDateUtc > DateTime.UtcNow);
            foreach (SignalDispatch<TKey> item in scheduledItems)
            {
                var signal = new SignalWrapper<SignalDispatch<TKey>>(item, isStored);
                AppendToTemporaryStorage(signal);
                ApplyResult(signal, ProcessingResult.ReturnToStorage);
            }

            IEnumerable<SignalDispatch<TKey>> immediateItems = items.Except(scheduledItems);
            foreach (SignalDispatch<TKey> item in immediateItems)
            {
                var signal = new SignalWrapper<SignalDispatch<TKey>>(item, isStored);
                Append(signal);
            }
        }

        public override void Append(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Append(item, item.Signal.DeliveryType);
        }

        public virtual SignalWrapper<SignalDispatch<TKey>> DequeueNext()
        {
            SignalWrapper<SignalDispatch<TKey>> item = null;
            List<int> activeDeliveryTypes = _dispatcherRegistry.GetActiveDeliveryTypes(true);

            lock (_queueLock)
            {
                foreach (KeyValuePair<int, Queue<SignalWrapper<SignalDispatch<TKey>>>> group in _itemsQueue)
                {
                    if (activeDeliveryTypes.Contains(group.Key) && group.Value.Count > 0)
                    {
                        item = group.Value.Dequeue();
                        break;
                    }
                }
            }

            return item;
        }

        public override void ApplyResult(SignalWrapper<SignalDispatch<TKey>> item, ProcessingResult result)
        {
            if (result == ProcessingResult.Success)
            {
                //successfuly sent dispatch
                _signalFlushJob.Delete(item);
            }
            else if (result == ProcessingResult.Fail)
            {
                //dispatcher throws error
                item.Signal.FailedAttempts++;
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(RetryPeriod);
                item.IsUpdated = true;

                _signalFlushJob.Return(item);
            }
            else if (result == ProcessingResult.Repeat)
            {
                //dispatcherer is not available
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(RetryPeriod);
                item.IsUpdated = true;
                Append(item, item.Signal.DeliveryType);
            }
            else if (result == ProcessingResult.NoHandlerFound)
            {
                //no dispatcher found matching diliveryType
                _logger.LogError(SenderInternalMessages.DispatchQueue_HandlerNotFound, item.Signal.DeliveryType);
                _signalFlushJob.Delete(item);
            }
            else if (result == ProcessingResult.ReturnToStorage)
            {
                //1. stoped Sender and saving everything from queue to database
                //2. insert schedules dispatch after it is composed
                _signalFlushJob.Return(item);
            }
        }
    }
}
