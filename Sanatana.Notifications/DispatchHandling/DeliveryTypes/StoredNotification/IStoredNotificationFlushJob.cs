using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DispatchHandling.DeliveryTypes.StoredNotification
{
    public interface IStoredNotificationFlushJob<TKey> 
        where TKey : struct
    {
        void Insert(StoredNotification<TKey> item);
    }
}