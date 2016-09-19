using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class SignalBounce<TKey>
        where TKey : struct
    {
        //свойства
        public virtual TKey SignalBounceID { get; set; }

        public virtual int DeliveryType { get; set; }

        public virtual TKey? ReceiverUserID { get; set; }
        public virtual string ReceiverAddress { get; set; }

        public virtual string MessageSubject { get; set; }
        public virtual string MessageBody { get; set; }
        public virtual DateTime? SendDateUtc { get; set; }

        public virtual BounceType BounceType { get; set; }
        public virtual string BounceDetailsXML { get; set; }
        public virtual DateTime BounceReceiveDateUtc { get; set; }    
    }
}
