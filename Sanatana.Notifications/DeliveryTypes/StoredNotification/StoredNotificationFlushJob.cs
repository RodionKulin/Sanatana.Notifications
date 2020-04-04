using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    public class StoredNotificationFlushJob<TKey> : IRegularJob, IStoredNotificationFlushJob<TKey> 
        where TKey : struct
    {
        //fields
        protected DateTime _lastFlushTimeUtc;
        protected IStoredNotificationQueries<TKey> _storedNotificationQueries;
        protected BlockingCollection<StoredNotification<TKey>> _flushQueue;


        //properties
        public TimeSpan FlushPeriod { get; set; }
        public int FlushQueueLimit { get; set; }


        //init
        public StoredNotificationFlushJob(SenderSettings senderSettings, IStoredNotificationQueries<TKey> storedNotificationQueries)
        {
            _storedNotificationQueries = storedNotificationQueries;
            _flushQueue = new BlockingCollection<StoredNotification<TKey>>();

            FlushPeriod = senderSettings.FlushJobFlushPeriod;
            FlushQueueLimit = senderSettings.FlushJobQueueLimit;
        }


        //IRegularJob methods
        public virtual void Tick()
        {
            bool isFlushRequired = CheckIsFlushRequired();
            if (isFlushRequired)
            {
                FlushQueue();
            }
        }

        public virtual void Flush()
        {
            FlushQueue();
        }


        //other methods
        protected virtual bool CheckIsFlushRequired()
        {
            DateTime nextFlushTimeUtc = _lastFlushTimeUtc + FlushPeriod;
            bool doScheduledQuery = nextFlushTimeUtc <= DateTime.UtcNow;

            bool hasItems = _flushQueue.Count > 0;

            bool hasMaxItems = _flushQueue.Count > FlushQueueLimit;

            return hasItems && (doScheduledQuery || hasMaxItems);
        }

        protected virtual void FlushQueue()
        {
            _lastFlushTimeUtc = DateTime.UtcNow;

            List<StoredNotification<TKey>> itemsToFlush = _flushQueue.ToList();
            _storedNotificationQueries.Insert(itemsToFlush);

            for (int i = 0; i < itemsToFlush.Count; i++)
            {
                _flushQueue.Take();
            }
        }

        public virtual void Insert(StoredNotification<TKey> item)
        {
            _flushQueue.Add(item);
        }
    }
}
