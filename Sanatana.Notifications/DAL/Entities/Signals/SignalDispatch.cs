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
        /// Identifier of Sender instance currently processing SignalDispatch.
        /// Setting LockedBy on SignalDispatch prevents it from being processed by another Sender instance in parallel.
        /// </summary>
        public Guid? LockedBy { get; set; }
        /// <summary>
        /// Time of lock start. Used to expire unreleased lock on SignalDispatch when Sender instance was not stopped gracefully.
        /// </summary>
        public DateTime? LockedSinceUtc { get; set; }



        /// <summary>
        /// SubscriberId that will receive this Dispatch
        /// </summary>
        public TKey? ReceiverSubscriberId { get; set; }
        /// <summary>
        /// Address of subscriber that will receive this Dispatch.
        /// </summary>
        public string ReceiverAddress { get; set; }
        /// <summary>
        /// Subscriber's Language setting used to fill Dispatch template.
        /// </summary>
        public string Language { get; set; }


        /// <summary>
        /// Identifier for EventSettings that is used during Dispatch building and consolidation. 
        /// </summary>
        public TKey? EventSettingsId { get; set; }
        /// <summary>
        /// Identifier for DispatchTemplate that is used to build Dispatch properties.
        /// </summary>
        public TKey? DispatchTemplateId { get; set; }


        /// <summary>
        /// DeliveryType that is used to match this Dispatch with IDispatcher that will send it.
        /// </summary>
        public int DeliveryType { get; set; }
        /// <summary>
        /// CategoryId that was used to compose this dispatch and match subscribers.
        /// </summary>
        public int? CategoryId { get; set; }
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
        /// EventData received with SignalEvent. Stored in json format.
        /// Is used for consolidation of multiple Dispatches for same subscriber into single Dispatch.
        /// </summary>
        public string TemplateData { get; set; }


        //methods
        public bool ShouldBeConsolidated()
        {
            return TemplateData != null         //TempalteData is present only on consolidated Dispatches
                && ReceiverSubscriberId != null //required for consolidation
                & CategoryId != null;           //required for consolidation
                                                //DeliveryType is also required, but it is not nullable
        }
    }
}
