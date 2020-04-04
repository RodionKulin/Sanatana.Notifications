using Sanatana.Notifications.DeliveryTypes.StoredNotification;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DispatchHandling.Limits;
using Sanatana.Notifications.DispatchHandling.Interrupters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Demo.Sender.Model
{
    public class StoredNotificationsDispatchChannel<TKey> : DispatchChannel<TKey>
        where TKey : struct
    {
        
        public StoredNotificationsDispatchChannel(IStoredNotificationDispatcher<TKey> storedNotificationsDispatcher)
        {
            Dispatcher = storedNotificationsDispatcher;
            LimitCounter = new NoLimitCounter();
            Interrupter = new ProgressiveTimeoutInterrupter<TKey>();
            DeliveryType = (int)DeliveryTypes.StoredNotifications;
        }
    }
}
