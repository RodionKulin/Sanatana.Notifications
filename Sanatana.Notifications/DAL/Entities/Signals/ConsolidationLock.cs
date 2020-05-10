using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.Entities
{
    public class ConsolidationLock<TKey> : IEquatable<ConsolidationLock<TKey>>
        where TKey : struct
    {
        public TKey ConsolidationLockId { get; set; }


        /// <summary>
        /// Identifier of Sender instance currently processing SignalDispatch.
        /// Setting LockedBy for group of consolidated SignalDispates prevents em from being processed by another Sender instance in parallel.
        /// </summary>
        public Guid? LockedBy { get; set; }
        /// <summary>
        /// Time of lock start. Used to expire unreleased lock on SignalDispatches consolidation when Sender instance was not stopped gracefully.
        /// </summary>
        public DateTime? LockedSinceUtc { get; set; }


        /// <summary>
        /// SubscriberId that will receive this Dispatch
        /// </summary>
        public TKey ReceiverSubscriberId { get; set; }
        /// <summary>
        /// DeliveryType that is used to match this Dispatch with IDispatcher that will send it.
        /// </summary>
        public int DeliveryType { get; set; }
        /// <summary>
        /// CategoryId that was used to compose this dispatch and match subscribers.
        /// </summary>
        public int CategoryId { get; set; }


        //not store in database
        /// <summary>
        /// SignalDispatchId that will start consolidation for ConsolidationRoot's category.
        /// Other SignalDispatches in same category and created before ConsolidationRoot's SendTimeUtc will be attached.
        /// </summary>
        public TKey ConsolidationRootId { get; set; }
        public DateTime ConsolidationRootSendDateUtc { get; set; }



        //methods
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((ConsolidationLock<TKey>)obj);
        }

        public virtual bool Equals(ConsolidationLock<TKey> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.CategoryId == CategoryId
                && other.DeliveryType == DeliveryType
                && EqualityComparer<TKey>.Default.Equals(other.ReceiverSubscriberId, ReceiverSubscriberId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = DeliveryType.GetHashCode();
                hashCode = (hashCode * 397) ^ CategoryId.GetHashCode();
                hashCode = (hashCode * 397) ^ ReceiverSubscriberId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ConsolidationLock<TKey> obj1, ConsolidationLock<TKey> obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null))
            {
                return false;
            }
            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.CategoryId == obj2.CategoryId
                && obj1.DeliveryType == obj2.DeliveryType
                && EqualityComparer<TKey>.Default.Equals(obj1.ReceiverSubscriberId, obj2.ReceiverSubscriberId);
        }

        public static bool operator !=(ConsolidationLock<TKey> obj1, ConsolidationLock<TKey> obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
