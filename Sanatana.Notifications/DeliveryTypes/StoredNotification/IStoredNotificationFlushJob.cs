using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    public interface IStoredNotificationFlushJob<TKey> 
        where TKey : struct
    {
        void Insert(StoredNotification<TKey> item);
    }
}