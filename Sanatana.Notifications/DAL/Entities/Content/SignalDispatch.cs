using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class SignalDispatch<TKey>
        where TKey : struct
    {
        public TKey SignalDispatchId { get; set; }

        public TKey? ReceiverSubscriberId { get; set; }
        public string ReceiverAddress { get; set; }

        public int DeliveryType { get; set; }
        public int? CategoryId { get; set; }
        public string TopicId { get; set; }

        public bool IsScheduled { get; set; }
        public int? ScheduleSet { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime SendDateUtc { get; set; }
        public int FailedAttempts { get; set; }
    }
}
