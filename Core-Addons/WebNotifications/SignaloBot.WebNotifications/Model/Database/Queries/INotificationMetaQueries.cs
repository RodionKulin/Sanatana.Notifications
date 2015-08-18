using SignaloBot.WebNotifications.Entities;
using System;

namespace SignaloBot.WebNotifications.Database.Queries
{
    public interface INotificationMetaQueries
    {
        void InsertNewType(int categoryID, string metaType, string metaKey, string metaValue, out Exception exception);
        System.Collections.Generic.List<NotificationMeta> Select(Guid userID, System.Collections.Generic.List<Notification> oldNotifies, out Exception exception);
    }
}
