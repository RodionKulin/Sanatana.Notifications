using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    [Serializable]
    public class SignalDispatch<TKey>
        where TKey : struct
    {
        /// <summary>
        /// Dispatch Id required to store in database.
        /// </summary>
        public TKey SignalDispatchId { get; set; }


        /// <summary>
        /// SubscriberId that will receive this Dispatch
        /// </summary>
        public TKey? ReceiverSubscriberId { get; set; }
        /// <summary>
        /// Address of subscriber that will receive this Dispatch.
        /// </summary>
        public string ReceiverAddress { get; set; }


        /// <summary>
        /// DeliveryType that is used to match this Dispatch with IDispatcher that will send it.
        /// </summary>
        public int DeliveryType { get; set; }
        /// <summary>
        /// CategoryId that was used to compose this dispatch and match subscribers.
        /// </summary>
        public int? CategoryId { get; set; }
        /// <summary>
        /// EventKey that was used to compose this dispatch and match subscribers.
        /// </summary>
        public int? EventKey { get; set; }
        /// <summary>
        /// TopicId that was used to compose this dispatch and match subscribers.
        /// </summary>
        public string TopicId { get; set; }


        /// <summary>
        /// Is dispatch scheduled to send after some time it was created.
        /// </summary>
        public bool IsScheduled { get; set; }
        /// <summary>
        /// Schedule set number used to match SubscriberScheduleSettings for concrete Dispatch.
        /// </summary>
        public int? ScheduleSet { get; set; }


        /// <summary>
        /// Date and time of Dispatch creation from Event or receving Dispatch from ISignalProvider.
        /// </summary>
        public DateTime CreateDateUtc { get; set; }
        /// <summary>
        /// Date and time when Dispatch will be send.
        /// </summary>
        public DateTime SendDateUtc { get; set; }
        /// <summary>
        /// Number of failed attempts to send dispatch when IDispatcher was available.
        /// </summary>
        public int FailedAttempts { get; set; }


        /// <summary>
        /// Enable storing dispatch in history database table after sending it.
        /// </summary>
        public bool StoreInHistory { get; set; }


        /// <summary>
        /// Before sending get all scheduled dispatches for subscriber of same category and create a single Dispatch from all of them.
        /// </summary>
        public bool Consolidate { get; set; }
    }
}
