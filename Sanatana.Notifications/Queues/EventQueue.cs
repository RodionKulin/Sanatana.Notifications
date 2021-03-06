﻿using System.Collections.Generic;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Sender;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Flushing.Queues;
using System.Linq;

namespace Sanatana.Notifications.Queues
{
    public class EventQueue<TKey> : QueueBase<SignalEvent<TKey>, TKey>, IEventQueue<TKey>
        where TKey : struct
    {
        //fields
        protected ISignalFlushJob<SignalEvent<TKey>> _signalFlushJob;
        protected ILogger _logger;

        //init
        public EventQueue(SenderSettings senderSettings, ITemporaryStorage<SignalEvent<TKey>> temporaryStorage
            , ISignalFlushJob<SignalEvent<TKey>> signalFlushJob, ILogger logger)
            : base(temporaryStorage)
        {
            _signalFlushJob = signalFlushJob;
            _logger = logger;

            PersistBeginOnItemsCount = senderSettings.SignalQueuePersistBeginOnItemsCount;
            PersistEndOnItemsCount = senderSettings.SignalQueuePersistEndOnItemsCount;
            IsTemporaryStorageEnabled = senderSettings.SignalQueueIsTemporaryStorageEnabled;

            _temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_EVENT_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };
        }


        //regular jobs methods
        public virtual void Tick()
        {
            ReturnExtraItems();
        }


        //queue methods
        public virtual void Append(List<SignalEvent<TKey>> items, bool isPermanentlyStored)
        {
            items.Select(x => SignalWrapper.Create(x, isPermanentlyStored))
                .ToList()
                .ForEach(Append);
        }

        public override void Append(SignalWrapper<SignalEvent<TKey>> item)
        {
            base.Append(item, item.Signal.EventKey);
        }

        public virtual SignalWrapper<SignalEvent<TKey>> DequeueNext()
        {
            SignalWrapper<SignalEvent<TKey>> item = null;

            lock (_queueLock)
            {
                foreach (KeyValuePair<int, Queue<SignalWrapper<SignalEvent<TKey>>>> group in _itemsQueue)
                {
                    if (group.Value.Count > 0)
                    {
                        item = group.Value.Dequeue();
                        break;
                    }
                }
            }
            
            return item;
        }

        public override void ApplyResult(SignalWrapper<SignalEvent<TKey>> item
            , ProcessingResult result)
        {
            if (result == ProcessingResult.Success)
            {
                _signalFlushJob.Delete(item);
            }
            else if (result == ProcessingResult.Fail)
            {
                item.Signal.FailedAttempts++;
                item.IsUpdated = true;

                _signalFlushJob.Return(item);
            }
            else if (result == ProcessingResult.Repeat)
            {
                //received only limited number of subscribers, need to repeat for next limited batch of subscribers
                Append(item);                
            }
            else if (result == ProcessingResult.NoHandlerFound)
            {
                _logger.LogError(SenderInternalMessages.EventQueue_HandlerNotFound, item.Signal.EventKey);
                _signalFlushJob.Delete(item);
            }
            else if (result == ProcessingResult.ReturnToStorage)
            {
                _signalFlushJob.Return(item);
            }
        }


    }
}
