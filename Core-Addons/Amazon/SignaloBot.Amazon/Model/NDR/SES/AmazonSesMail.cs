using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.NDR.SES
{
    internal class AmazonSesMail
    {
        //свойства
        public string Timestamp { get; set; }
        public string Source { get; set; }
        public string MessageId { get; set; }
        public List<string> Destination { get; set; }

    }
}
