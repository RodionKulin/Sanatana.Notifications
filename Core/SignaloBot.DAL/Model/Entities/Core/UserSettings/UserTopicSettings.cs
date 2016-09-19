using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SignaloBot.DAL
{
    public class UserTopicSettings<TKey>
        where TKey : struct
    {
        //свойства
        public TKey UserID { get; set; }
        public int CategoryID { get; set; }
        public string TopicID { get; set; }

        public DateTime AddDateUtc { get; set; }
        public DateTime? LastSendDateUtc { get; set; }

        public int SendCount { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
    }
}