using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SignaloBot.WebNotifications.Entities.Results
{
    internal class NotificationTotal
    {
        //свойства
        public Guid NotificationID { get; set; }
        public Guid UserID { get; set; }
        public int CategoryID { get; set; }
        public string TopicID { get; set; }
        public string NotifyText { get; set; }
        public DateTime SendDateUtc { get; set; }
        public bool IsDirty { get; set; }
        public int Variant { get; set; }
        public string Culture { get; set; }    
        public string Tag { get; set; }
        

        


        //Total
        public long TotalRows { get; set; }



        public static explicit operator Notification(NotificationTotal t)
        {
            return new Notification()
            {
                NotificationID = t.NotificationID,
                UserID = t.UserID,
                CategoryID = t.CategoryID,
                TopicID = t.TopicID,
                NotifyText = t.NotifyText,
                SendDateUtc = t.SendDateUtc,
                IsDirty = t.IsDirty,
                Variant = t.Variant,
                Culture = t.Culture,
                Tag = t.Tag
            };
        }
    }
}