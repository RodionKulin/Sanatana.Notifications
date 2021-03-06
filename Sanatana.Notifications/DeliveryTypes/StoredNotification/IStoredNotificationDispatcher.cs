﻿using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.Processing;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    public interface IStoredNotificationDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
    }
}