using SignaloBot.TestParameters.Model;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SignaloBot.WebNotifications.Entities;

namespace SignaloBot.WebNotifications.Tests.Model
{
    public class NotificationsTestCommonCreator
    {
        public static Notification CreateNotification()
        {
            return new Notification()
            {
                NotificationID = SequentialGuid.NewGuid(),
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                UserID = SignaloBotTestParameters.ExistingUserID,
                TopicID = SignaloBotTestParameters.ExistingSubscriptionTopicID,
                SendDateUtc = DateTime.UtcNow,
                NotifyText = "notification text 1",
                IsDirty = false,
                Tag = "Notification Tag 1",
                Culture = Thread.CurrentThread.CurrentCulture.Name
            };
        }

        public static NotificationMeta CreateNotificationMeta(Guid notifyID, string metaType = null)
        {
            if (metaType == null)
                metaType = "ArticleName";

            return new NotificationMeta()
            {
                NotificationMetaID = SequentialGuid.NewGuid(),
                NotificationID = notifyID,
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                UserID = SignaloBotTestParameters.ExistingUserID,
                TopicID = SignaloBotTestParameters.ExistingSubscriptionTopicID,
                MetaType = metaType,
                MetaKey = "1",
                MetaValue = "Who Frame Roger Rabit"
            };
        }

    }
}
