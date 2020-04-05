
using Sanatana.Notifications.DispatchHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DeliveryTypes.StoredNotification;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    public class StoredNotificationDispatcher<TKey> : IDispatcher<TKey>, IStoredNotificationDispatcher<TKey>
        where TKey : struct
    {
        //fields
        protected IStoredNotificationFlushJob<TKey> _storedNotificationsFlushJob;


        //init
        public StoredNotificationDispatcher(IStoredNotificationFlushJob<TKey> storedNotificationsFlushJob)
        {
            _storedNotificationsFlushJob = storedNotificationsFlushJob;
        }


        //methods
        public virtual Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            var signal = (StoredNotificationDispatch<TKey>)item;

            var storedNotification = new StoredNotification<TKey>
            {
                MessageBody = signal.MessageBody,
                MessageSubject = signal.MessageSubject,
                CategoryId = signal.CategoryId,
                TopicId = signal.TopicId,
                SubscriberId = signal.ReceiverSubscriberId.Value,
                CreateDateUtc = DateTime.UtcNow
            };
            _storedNotificationsFlushJob.Insert(storedNotification);

            return Task.FromResult(ProcessingResult.Success);
        }

        public virtual Task<DispatcherAvailability> CheckAvailability()
        {
            return Task.FromResult(DispatcherAvailability.Available);
        }

        public virtual void Dispose()
        {

        }
    }
}
