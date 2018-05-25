using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS.SES
{
    public class AmazonSesMail
    {
        //properties
        public string Timestamp { get; set; }
        public string Source { get; set; }
        public string MessageId { get; set; }
        public List<string> Destination { get; set; }

    }
}
