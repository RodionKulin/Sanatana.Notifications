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
    public class SignalEventFlushJob<TKey> : SignalFlushJobBase<SignalEvent<TKey>>
        where TKey : struct
    {

        //init
        public SignalEventFlushJob(SenderSettings senderSettings, ITemporaryStorage<SignalEvent<TKey>> temporaryStorage
            , ISignalEventQueries<TKey> queries)
            : base(temporaryStorage, queries)
        {
            FlushPeriod = senderSettings.FlushJobFlushPeriod;
            QueueLimit = senderSettings.FlushJobQueueLimit;
            IsTemporaryStorageEnabled = senderSettings.SignalQueueIsTemporaryStorageEnabled;

            _temporaryStorageParameters = new TemporaryStorageParameters()
            {
                QueueType = NotificationsConstants.TS_EVENT_QUEUE_KEY,
                EntityVersion = NotificationsConstants.TS_ENTITIES_VERSION
            };
        }

    }
}
