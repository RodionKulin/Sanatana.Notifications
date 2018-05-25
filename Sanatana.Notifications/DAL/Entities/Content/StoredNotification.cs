using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class StoredNotification<TKey>
        where TKey : struct
    {
        public TKey StoredNotificationId { get; set; }
        public TKey SubscriberId { get; set; }
        public int? CategoryId { get; set; }
        public string TopicId { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
    }
}
