using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class Subscriber<TKey>
        where TKey : struct
    {
        public TKey UserID { get; set; }
        public int DeliveryType { get; set; }
        public string Address { get; set; }
        public string TimeZoneID { get; set; }
        public string Language { get; set; }
    }
}
