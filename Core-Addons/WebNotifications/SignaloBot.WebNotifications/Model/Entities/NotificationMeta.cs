using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignaloBot.WebNotifications.Entities
{
    public class NotificationMeta
    {
        public Guid NotificationMetaID { get; set; }
        public Guid NotificationID { get; set; }
        public Guid UserID { get; set; }
        public int CategoryID { get; set; }
        public string TopicID { get; set; }        
        public string MetaType { get; set; }        
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }



        //foreign key
        internal virtual Notification Notification { get; set; }


        //инициализация
        public static NotificationMeta Create(Guid userID, int categoryID, string topicID,
            string type, string key, string value)
        {
            return new NotificationMeta()
            {
                UserID = userID,
                CategoryID = categoryID,
                TopicID = topicID,
                MetaType = type,
                MetaKey = key,
                MetaValue = value
            };
        }
    }
}