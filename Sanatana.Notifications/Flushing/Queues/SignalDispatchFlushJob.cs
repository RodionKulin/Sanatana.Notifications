using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
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

        //init
        public SignalDispatchFlushJob(SenderSettings senderSettings, ITemporaryStorage<SignalDispatch<TKey>> temporaryStorage
            , ISignalDispatchQueries<TKey> queries)
            : base(senderSettings, temporaryStorage, queries)
        {
            _dispatchQueries = queries;

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


        //enqueue methods
        public override void Delete(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Delete(item);

            bool isConsolidated = item.Signal.TemplateData != null;
            if (isConsolidated)
            {
                _flushQueues[FlushAction.DeleteConsolidated].Queue.Add(item);
            }
        }

        public override void Return(SignalWrapper<SignalDispatch<TKey>> item)
        {
            base.Return(item);

            bool isConsolidated = item.Signal.TemplateData != null;
            //Assume IsUpdated can only be set if consolidation happened.
            //Otherwise would need to introduce new property SignalWrapper.ConsolidationCompleted.
            if (isConsolidated && item.IsUpdated)
            {
                _flushQueues[FlushAction.DeleteConsolidated].Queue.Add(item);
            }
        }
    }
}
