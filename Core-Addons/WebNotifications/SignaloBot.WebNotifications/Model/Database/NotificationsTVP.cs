using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using System.Data.SqlClient;
using Common.EntityFramework;
using SignaloBot.WebNotifications.Entities;

namespace SignaloBot.WebNotifications.Database
{
    public class NotificationsTVP
    {       
        //названия табличных типов        
        public const string NOTIFICATION_META_TYPE = "NotificationMetaType";        
        public const string NOTIFICATION_TYPE = "NotificationType";


        //методы      
        public static SqlParameter ToNotificationMetaType(string paramName, List<NotificationMeta> items, string prefix)
        {
            DataTable dataTable = items.ToDataTable(new List<string>()
            {
                ReflectionExtensions.GetPropertyName((NotificationMeta t) => t.NotificationMetaID),
                ReflectionExtensions.GetPropertyName((NotificationMeta t) => t.NotificationID),
                ReflectionExtensions.GetPropertyName((NotificationMeta t) => t.UserID),
                ReflectionExtensions.GetPropertyName((NotificationMeta t) => t.MetaType),
                ReflectionExtensions.GetPropertyName((NotificationMeta t) => t.MetaKey),
                ReflectionExtensions.GetPropertyName((NotificationMeta t) => t.MetaValue)
            });   

            SqlParameter param = new SqlParameter(paramName, dataTable);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = prefix + NOTIFICATION_META_TYPE;

            return param;
        }


       
    }
}
