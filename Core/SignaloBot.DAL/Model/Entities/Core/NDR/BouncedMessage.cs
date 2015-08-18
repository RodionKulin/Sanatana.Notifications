using SignaloBot.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities
{
    public class BouncedMessage
    {
        //свойства
        public Guid BouncedMessageID { get; set; }

        public int DeliveryType { get; set; }

        public Guid? ReceiverUserID { get; set; }
        public string ReceiverAddress { get; set; }

        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public DateTime? SendDateUtc { get; set; }

        public BounceType BounceType { get; set; }
        public string BounceDetailsXML { get; set; }
        public DateTime BounceReceiveDateUtc { get; set; }    
    }
}
