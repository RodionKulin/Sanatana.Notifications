using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class UpdateSubscriberParameter
    {
        public long SubscriberId { get; set; }
        public int SendCount { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryId { get; set; }
        public string TopicId { get; set; }
    }
}
