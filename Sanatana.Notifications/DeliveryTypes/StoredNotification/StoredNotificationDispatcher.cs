
using Sanatana.Notifications.Dispatching;
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
        protected ILogger _logger;
        protected IStoredNotificationFlushJob<TKey> _storedNotificationsFlushJob;


        //init
        public StoredNotificationDispatcher(ILogger logger, IStoredNotificationFlushJob<TKey> storedNotificationsFlushJob)
        {
            _logger = logger;
            _storedNotificationsFlushJob = storedNotificationsFlushJob;
        }


        //methods
        public Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            if ((item is StoredNotificationDispatch<TKey>) == false)
            {
                _logger.LogError(SenderInternalMessages.Dispatcher_WrongInputType
                    , item.GetType(), GetType(), typeof(StoredNotificationDispatch<TKey>));
                return Task.FromResult(ProcessingResult.Fail);
            }
            var signal = item as StoredNotificationDispatch<TKey>;

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

        public Task<DispatcherAvailability> CheckAvailability()
        {
            return Task.FromResult(DispatcherAvailability.Available);
        }

        public void Dispose()
        {

        }
    }
}
