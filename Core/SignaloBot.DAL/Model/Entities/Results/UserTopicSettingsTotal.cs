using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Results
{
    internal class UserTopicSettingsTotal
    {
        //свойства
        public Guid UserID { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryID { get; set; }
        public int TopicID { get; set; }

        public DateTime AddDateUtc { get; set; }
        public DateTime? LastSendDateUtc { get; set; }

        public int SendCount { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }


        //Total
        public long TotalRows { get; set; }


        
        public static explicit operator UserTopicSettings(UserTopicSettingsTotal t)
        {
            return new UserTopicSettings()
            {
                UserID = t.UserID,
                DeliveryType = t.DeliveryType,
                CategoryID = t.CategoryID,
                TopicID = t.TopicID,

                AddDateUtc = t.AddDateUtc,
                LastSendDateUtc = t.LastSendDateUtc,

                SendCount = t.SendCount,
                IsEnabled = t.IsEnabled,
                IsDeleted = t.IsDeleted
            };
        }
    }
}
