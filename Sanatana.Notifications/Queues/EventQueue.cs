using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Dispatching;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Sender;

namespace Sanatana.Notifications.Queues
{
    public class EventQueue<TKey> : QueueBase<SignalEvent<TKey>, TKey>
        , IEventQueue<TKey>
        where TKey : struct
    {
        //fields
        protected ISignalFlushJob<SignalEvent<TKey>> _signalFlushJob;

        //init
        public EventQueue(SenderSettings senderSettings, ITemporaryStorage<SignalEvent<TKey>> temporaryStorage
            , ISignalFlushJob<SignalEvent<TKey>> signalFlushJob)
            : base(temporaryStorage)
        {
            _signalFlushJob = signalFlushJob;

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
        public virtual void Append(List<SignalEvent<TKey>> items, bool isStored)
        {
            foreach (SignalEvent<TKey> item in items)
            {
                var signal = new SignalWrapper<SignalEvent<TKey>>(item, isStored);
                Append(signal);
            }
        }

        public override void Append(SignalWrapper<SignalEvent<TKey>> item)
        {
            base.Append(item, item.Signal.CategoryId);
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
                Append(item);                
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
