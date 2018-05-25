using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing
{
    public class SignalDispatchFlushJob<TKey> : SignalFlushJobBase<SignalDispatch<TKey>>
        where TKey : struct
    {

        //init
        public SignalDispatchFlushJob(SenderSettings senderSettings, ITemporaryStorage<SignalDispatch<TKey>> temporaryStorage
            , ISignalDispatchQueries<TKey> queries)
            : base(temporaryStorage, queries)
        {
            FlushPeriod = senderSettings.FlushJobFlushPeriod;
            QueueLimit = senderSettings.FlushJobQueueLimit;
            IsTemporaryStorageEnabled = senderSettings.SignalQueueIsTemporaryStorageEnabled;

            _temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_DISPATCH_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };
        }

    }
}
