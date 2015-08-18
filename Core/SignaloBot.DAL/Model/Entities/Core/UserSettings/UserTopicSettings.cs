using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SignaloBot.DAL.Entities.Core
{
    public class UserTopicSettings
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
    }
}