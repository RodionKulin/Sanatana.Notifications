using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Sanatana.Notifications.DAL;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public static class TableValuedParameters
    {
        //parameter names
        public const string UPDATE_SUBSCRIBERS_PARAMETER_NAME = "@UpdateSubscribers";


        //TVP names
        public const string DISPATCH_UPDATE_TYPE = "SignalDispatchType";
        public const string EVENT_UPDATE_TYPE = "SignalEventType";
        public const string STORED_NOTIFICATION_TYPE = "StoredNotificationType";
        public const string SUBSCRIBER_TYPE = "SubscriberType";
        public const string SUBSCRIBER_CATEGORY_SETTINGS_TYPE_IS_ENABLED = "SubscriberCategorySettingsTypeIsEnabled";
        public const string SUBSCRIBER_CATEGORY_SETTINGS_TYPE = "SubscriberCategorySettingsType";
        public const string SUBSCRIBER_TOPIC_SETTINGS_TYPE = "SubscriberTopicSettingsType";
        public const string SUBSCRIBER_TOPIC_SETTINGS_TYPE_IS_ENABLED = "SubscriberTopicSettingsTypeIsEnabled";
        public const string SUBSCRIBER_NDR_SETTINGS_TYPE = "SubscriberNDRSettingsType";
        public const string EVENT_SETTINGS_TYPE = "EventSettingsType";
        public const string DISPATCH_TEMPLATE_TYPE = "DispatchTemplateType";


        //methods
        public static string GetFullTVPName(string schema, string name)
        {
            return $"[{schema}].[{name}]";
        }
       

    }
}
