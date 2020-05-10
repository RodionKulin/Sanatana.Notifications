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
        protected ILockTracker<TKey> _dispatchLockTracker;
        protected IConsolidationLockTracker<TKey> _consolidationLockTracker;


        //init
        public SignalDispatchFlushJob(SenderSettings senderSettings, ITemporaryStorage<SignalDispatch<TKey>> temporaryStorage,
            ISignalDispatchQueries<TKey> dispatchQueries, ILockTracker<TKey> dispatchLockTracker,
            IConsolidationLockTracker<TKey> consolidationLockTracker)
            : base(senderSettings, temporaryStorage, dispatchQueries)
        {
            _dispatchQueries = dispatchQueries;
            _dispatchLockTracker = dispatchLockTracker;
            _consolidationLockTracker = consolidationLockTracker;

            IsTemporaryStorageEnabled = senderSettings.SignalQueueIsTemporaryStorageEnabled;
            _temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_DISPATCH_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };

            //how to flush items
            _flushQueues[FlushAction.DeleteConsolidated] = new FlushQueue<SignalWrapper<SignalDispatch<TKey>>>(signals => DeleteConsolidatedDispatches(signals));

            //what to do after flushing
            _flushedItemsHandlers.Add(ForgetDispatchLocks);
            _flushedItemsHandlers.Add(ForgetConsolidationLocks);
        }


        //flush queries methods
        protected virtual Task DeleteConsolidatedDispatches(List<SignalWrapper<SignalDispatch<TKey>>> items)
        {
            List<SignalDispatch<TKey>> signals = items.Select(x => x.Signal).ToList();
            return _dispatchQueries.DeleteConsolidated(signals);
        }


        //flush complete handlers
        protected virtual void ForgetDispatchLocks(List<SignalWrapper<SignalDispatch<TKey>>> flushedItems)
        {
            var ids = flushedItems.Select(x => x.Signal.SignalDispatchId);
            _dispatchLockTracker.ForgetLocks(ids);

            ids = flushedItems
                .Where(x => x.ConsolidatedSignals != null)
                .SelectMany(x => x.ConsolidatedSignals)
                .Select(x => x.Signal.SignalDispatchId);
            _dispatchLockTracker.ForgetLocks(ids);
        }

        protected virtual void ForgetConsolidationLocks(List<SignalWrapper<SignalDispatch<TKey>>> flushedItems)
        {
            IEnumerable<SignalDispatch<TKey>> signals = flushedItems
                .Where(x => x.IsConsolidationCompleted)
                .Select(x => x.Signal);
            _consolidationLockTracker.ForgetLocks(signals);
        }


        //enqueue dispatches for flushing methods
        public override void Delete(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Delete(item);

            if (item.IsConsolidationCompleted)
            {
                EnqueueItem(item, FlushAction.DeleteConsolidated);
            }
        }

        public override void Return(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Return(item);

            if (item.IsConsolidationCompleted)
            {
                EnqueueItem(item, FlushAction.DeleteConsolidated);
            }
        }
    }
}
