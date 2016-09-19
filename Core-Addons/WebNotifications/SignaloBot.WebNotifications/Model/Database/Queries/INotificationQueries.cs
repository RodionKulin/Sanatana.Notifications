using SignaloBot.WebNotifications.Entities;
using System;
using System.Collections.Generic;

namespace SignaloBot.WebNotifications.Database.Queries
{
    public interface INotificationQueries
    {
        int DeleteCategory(Guid userID, int categoryID, out Exception exception);
        int DeleteTag(Guid userID, int categoryID, string tag, out Exception exception);
        int DeleteTopic(Guid userID, int categoryID, string topicID, out Exception exception);
        void Insert(Notification notification, System.Collections.Generic.List<NotificationMeta> notifyMetas, System.Collections.Generic.List<Guid> userIDs, out Exception exception);
        System.Collections.Generic.List<Notification> SelectUpdateLastVisit(Guid userID, bool updateVisit, out int total, int page, int itemsOnPage, out Exception exception);
        void UpdateDirty(System.Collections.Generic.List<Notification> notifies, out Exception exception);
        void Upsert(Notification notification, List<NotificationMeta> notifyMetas, List<Guid> userIDs, out Exception exception);
    }
}
