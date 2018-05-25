using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Results
{
    public class Subscriber<TKey>
        where TKey : struct
    {
        public TKey SubscriberId { get; set; }
        public int DeliveryType { get; set; }
        public string Address { get; set; }
        public string TimeZoneId { get; set; }
        public string Language { get; set; }
    }
}
