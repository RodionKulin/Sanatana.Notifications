using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.DispatchHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq.Expressions;
using Sanatana.Notifications;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Sender;

namespace Sanatana.Notifications.Queues
{
    public class DispatchQueue<TKey> : QueueBase<SignalDispatch<TKey>, TKey>
        , IDispatchQueue<TKey>
        where TKey : struct
    {
        //fields
        protected IDispatchChannelRegistry<TKey> _dispatcherRegistry;
        protected ISignalFlushJob<SignalDispatch<TKey>> _signalFlushJob;


        //properties
        /// <summary>
        /// Pause duration after failed attempt before retrying.
        /// </summary>
        public TimeSpan FailedAttemptRetryPeriod { get; set; }


        //init
        public DispatchQueue(SenderSettings senderSettings, ITemporaryStorage<SignalDispatch<TKey>> temporaryStorage
            , IDispatchChannelRegistry<TKey> dispatcherRegistry, ISignalFlushJob<SignalDispatch<TKey>> signalFlushJob)
            : base(temporaryStorage)
        {
            _dispatcherRegistry = dispatcherRegistry;
            _signalFlushJob = signalFlushJob;

            FailedAttemptRetryPeriod = senderSettings.SignalQueueOnFailedAttemptRetryPeriod;
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

        public override void ApplyResult(SignalWrapper<SignalDispatch<TKey>> item
            , ProcessingResult result)
        {
            if (result == ProcessingResult.Success)
            {
                _signalFlushJob.Delete(item);
            }
            else if (result == ProcessingResult.Fail)
            {
                item.Signal.FailedAttempts++;
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(FailedAttemptRetryPeriod);
                item.IsUpdated = true;

                _signalFlushJob.Return(item);
            }
            else if (result == ProcessingResult.Repeat)
            {
                Append(item, item.Signal.DeliveryType);
            }
            else if (result == ProcessingResult.NoHandlerFound)
            {
                _signalFlushJob.Delete(item);
            }
            else if (result == ProcessingResult.ReturnToStorage)
            {
                _signalFlushJob.Return(item);
            }
        }
    }
}
