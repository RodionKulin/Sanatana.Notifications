using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Results
{
    public class Subscriber
    {
        public Guid UserID { get; set; }
        public string Address { get; set; }
        public string TimeZoneID { get; set; }
    }
}
