using Sanatana.Notifications.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class SignalBounce<TKey>
        where TKey : struct
    {
        //properties
        public TKey SignalBounceId { get; set; }

        public int DeliveryType { get; set; }

        public TKey? ReceiverSubscriberId { get; set; }
        public string ReceiverAddress { get; set; }

        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public DateTime? SendDateUtc { get; set; }

        public BounceType BounceType { get; set; }
        public string BounceDetailsXML { get; set; }
        public DateTime BounceReceiveDateUtc { get; set; }    
    }
}
