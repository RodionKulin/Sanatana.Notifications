using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class SignalDispatchBase<TKey>
        where TKey : struct
    {
        public virtual TKey SignalDispatchID { get; set; }

        public virtual int DeliveryType { get; set; }
        public virtual int CategoryID { get; set; }
        public virtual string TopicID { get; set; }

        public virtual DateTime SendDateUtc { get; set; }
        public virtual bool IsDelayed { get; set; }
        public virtual int FailedAttempts { get; set; }

        public virtual TKey? ReceiverUserID { get; set; }
        public virtual string ReceiverAddress { get; set; }
        public virtual string ReceiverDisplayName { get; set; }

    }
}
