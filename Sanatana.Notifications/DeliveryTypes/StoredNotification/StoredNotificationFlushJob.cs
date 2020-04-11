using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Flushing;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    public class StoredNotificationFlushJob<TKey> : FlushJobBase<StoredNotification<TKey>>, IStoredNotificationFlushJob<TKey> 
        where TKey : struct
    {
        //init
        public StoredNotificationFlushJob(SenderSettings senderSettings, IStoredNotificationQueries<TKey> queries)
            : base(senderSettings)
        {
            _flushQueues[FlushAction.Insert] = new FlushQueue<StoredNotification<TKey>>(items => queries.Insert(items));
        }


        //add to flush queue
        public virtual void Insert(StoredNotification<TKey> item)
        {
            _flushQueues[FlushAction.Insert].Queue.Add(item);
        }
    }
}
