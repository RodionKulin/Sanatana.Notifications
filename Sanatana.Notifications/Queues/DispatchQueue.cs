using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Sender;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Flushing.Queues;
using Sanatana.Notifications.Resources;

namespace Sanatana.Notifications.Queues
{
    public class DispatchQueue<TKey> : QueueBase<SignalDispatch<TKey>, TKey>, IDispatchQueue<TKey>
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
        /// <summary>
        /// Maximum number of failed attempts after which item won't be fetched from permanent storage any more.
        /// </summary>
        public int MaxFailedAttempts { get; set; }


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
            MaxFailedAttempts = senderSettings.DatabaseSignalProviderItemsMaxFailedAttempts;
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


        //append methods
        public virtual void Append(List<SignalDispatch<TKey>> signals, bool isPermanentlyStored)
        {
            SignalWrapper<SignalDispatch<TKey>>[] items = signals
                .Select(x => SignalWrapper.Create(x, isPermanentlyStored))
                .ToArray();

            //return to database scheduled dispatches
            SignalWrapper<SignalDispatch<TKey>>[] scheduledItems = items
                .Where(x => x.Signal.IsScheduled && x.Signal.SendDateUtc > DateTime.UtcNow)
                .ToArray();
            foreach (SignalWrapper<SignalDispatch<TKey>> item in scheduledItems)
            {
                AppendToTemporaryStorage(item);
                ApplyResult(item, ProcessingResult.ReturnToStorage);
            }

            //consolidate and enqueue immediate dispatches
            List<SignalWrapper<SignalDispatch<TKey>>> immediateItems = items.Except(scheduledItems).ToList();
            immediateItems = Consolidate(immediateItems);
            immediateItems.ForEach(Append);
        }

        protected virtual List<SignalWrapper<SignalDispatch<TKey>>> Consolidate(
            List<SignalWrapper<SignalDispatch<TKey>>> items)
        {
            List<SignalWrapper<SignalDispatch<TKey>>> newList = items
                .Where(x => !x.Signal.ShouldBeConsolidated())
                .ToList();

            items.Where(x => x.Signal.ShouldBeConsolidated())
                .GroupBy(x => new
                {
                    x.Signal.ReceiverSubscriberId,
                    x.Signal.CategoryId,
                    x.Signal.SignalDispatchId
                })
                .Select(x =>
                {
                    SignalWrapper<SignalDispatch<TKey>> signal = x.First();
                    signal.ConsolidatedSignals = x.ToArray();
                    return signal;
                })
                .ToList()
                .ForEach(newList.Add);

            return newList;
        }

        public override void Append(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Append(item, item.Signal.DeliveryType);
        }


        //dequeue methods
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



        //apply result methods
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
                IncrementFailedAttempts(item);
                Unlock(item);
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(RetryPeriod);
                item.IsUpdated = true;
                _signalFlushJob.Return(item);
            }
            else if (result == ProcessingResult.Repeat)
            {
                //dispatcher is not available
                Unlock(item);
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(RetryPeriod);
                item.IsUpdated = true;
                _signalFlushJob.Return(item);
            }
            else if (result == ProcessingResult.NoHandlerFound)
            {
                //no dispatcher found matching deliveryType
                IncrementFailedAttempts(item);
                Unlock(item);
                item.Signal.SendDateUtc = DateTime.UtcNow.Add(RetryPeriod);
                item.IsUpdated = true;
                _signalFlushJob.Return(item);
            }
            else if (result == ProcessingResult.ReturnToStorage)
            {
                //1. stoped Sender and saving everything from queue to database
                //2. insert scheduled dispatch after it is composed
                Unlock(item);
                _signalFlushJob.Return(item);
            }
        }

        protected virtual void IncrementFailedAttempts(SignalWrapper<SignalDispatch<TKey>> item)
        {
            item.Signal.FailedAttempts++;
            if (item.Signal.FailedAttempts >= MaxFailedAttempts)
            {
                _logger.LogError(SenderInternalMessages.DispatchQueue_MaxAttemptsReached,
                    MaxFailedAttempts, nameof(SignalDispatch<TKey>), item.Signal.SignalDispatchId);
            }
        }

        protected virtual void Unlock(SignalWrapper<SignalDispatch<TKey>> item)
        {
            if(item.Signal.LockedBy == null && item.Signal.LockedSinceUtc == null)
            {
                return;
            }

            item.Signal.LockedBy = null;
            item.Signal.LockedSinceUtc = null;
            item.IsUpdated = true;
        }
    }
}
