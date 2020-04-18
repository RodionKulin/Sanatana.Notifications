using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Locking;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing.Queues
{
    public class SignalDispatchFlushJob<TKey> : SignalFlushJobBase<SignalDispatch<TKey>>
        where TKey : struct
    {
        //fields
        protected ISignalDispatchQueries<TKey> _dispatchQueries;
        protected ILockTracker<TKey> _lockTracker;


        //init
        public SignalDispatchFlushJob(SenderSettings senderSettings, ITemporaryStorage<SignalDispatch<TKey>> temporaryStorage
            , ISignalDispatchQueries<TKey> queries, ILockTracker<TKey> lockTracker)
            : base(senderSettings, temporaryStorage, queries)
        {
            _dispatchQueries = queries;
            _lockTracker = lockTracker;

            IsTemporaryStorageEnabled = senderSettings.SignalQueueIsTemporaryStorageEnabled;
            _temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_DISPATCH_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };

            _flushQueues[FlushAction.DeleteConsolidated] = new FlushQueue<SignalWrapper<SignalDispatch<TKey>>>(
                signals => DeleteConsolidated(signals));
        }


        //flush queries methods
        protected virtual Task DeleteConsolidated(List<SignalWrapper<SignalDispatch<TKey>>> items)
        {
            List<SignalDispatch<TKey>> signals = items.Select(x => x.Signal).ToList();
            return _dispatchQueries.DeleteConsolidated(signals);
        }

        protected override List<SignalWrapper<SignalDispatch<TKey>>> FlushQueues()
        {
            List<SignalWrapper<SignalDispatch<TKey>>> flushedItems = base.FlushQueues();

            _lockTracker.ForgetLock(flushedItems.Select(x => x.Signal.SignalDispatchId));

            return flushedItems;
        }



        //enqueue methods
        public override void Delete(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Delete(item);

            if (item.IsConsolidationCompleted)
            {
                _flushQueues[FlushAction.DeleteConsolidated].Queue.Add(item);
            }
        }

        public override void Return(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Return(item);

            if (item.IsConsolidationCompleted)
            {
                _flushQueues[FlushAction.DeleteConsolidated].Queue.Add(item);
            }
        }
    }
}
