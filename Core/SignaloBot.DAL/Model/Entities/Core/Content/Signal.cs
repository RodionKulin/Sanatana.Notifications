using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Core
{
    public class Signal : IMessage
    {
        public Guid SignalID { get; set; }

        public int DeliveryType { get; set; }
        public int CategoryID { get; set; }
        public int? TopicID { get; set; }

        public Guid? ReceiverUserID { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverDisplayName { get; set; }

        public string SenderAddress { get; set; }
        public string SenderDisplayName { get; set; }

        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public bool IsBodyHtml { get; set; }

        public DateTime SendDateUtc { get; set; }
        public bool IsDelayed { get; set; }
        public int FailedAttempts { get; set; }
    }
}
