using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public static class DefaultTableNameConstants
    {
        public const string SubscriberDeliveryTypeSettings = "SubscriberDeliveryTypeSettings";
        public const string SubscriberCategorySettings = "SubscriberCategorySettings";
        public const string SubscriberTopicSettings = "SubscriberTopicSettings";
        public const string SubscriberScheduleSettings = "SubscriberScheduleSettings";

        public const string SignalEvents = "SignalEvents";
        public const string SignalDispatches = "SignalDispatches";
        public const string SignalBounces = "SignalBounces";
        public const string StoredNotifications = "StoredNotifications";

        public const string EventSettings = "EventSettings";
        public const string DispatchTemplates = "DispatchTemplates";
    }
}
