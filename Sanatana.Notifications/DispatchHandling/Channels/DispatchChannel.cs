using Sanatana.Notifications.DispatchHandling.Interrupters;
using Sanatana.Notifications.DispatchHandling.Limits;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Models;

namespace Sanatana.Notifications.DispatchHandling.Channels
{
    public class DispatchChannel<TKey> : IDispatchChannel<TKey> 
        where TKey : struct
    {
        //fields
        /// <summary>
        /// Time of the limits end for this channel.
        /// </summary>
        protected DateTime? _nextLimitsEndUtc;
        /// <summary>
        /// Time of the penalty end for this channel.
        /// </summary>
        protected DateTime? _nextTimeoutEndUtc;


        //properties
        /// <summary>
        /// Identifier of delivery type that is used to match Dispatch to DispatchChannel.
        /// </summary>
        public int DeliveryType { get; set; }
        /// <summary>
        /// Dispatch message sender.
        /// </summary>
        public IDispatcher<TKey> Dispatcher { get; set; }
        /// <summary>
        /// Message counter, that can limit number of messages. 
        /// For example limit number of messages per period of time with PeriodLimitCounter.
        /// </summary>
        public ILimitCounter LimitCounter { get; set; }
        /// <summary>
        /// Message successive and failed attempts counter, that can interrupt DispatchChannel from processing Dispatches.
        /// For example introduce progressive timeout on failed attempts with ProgressiveTimeoutInterrupter.
        /// </summary>
        public IInterrupter<TKey> Interrupter { get; set; }
        /// <summary>
        /// Amount of deliveries before reaching the limit. 
        /// </summary>
        public int AvailableLimitCapacity { get; set; }

        //dependent properties
        public virtual bool IsActive
        {
            get
            {
                return (_nextLimitsEndUtc == null || _nextLimitsEndUtc <= DateTime.UtcNow)
                    && (_nextTimeoutEndUtc == null || _nextTimeoutEndUtc <= DateTime.UtcNow);
            }
        }



        //init
        public DispatchChannel()
        {
            LimitCounter = new NoLimitCounter();
            Interrupter = new ProgressiveTimeoutInterrupter<TKey>();
        }
        public DispatchChannel(int deliveryType, IDispatcher<TKey> dispatcher)
            : this()
        {
            DeliveryType = deliveryType;
            Dispatcher = dispatcher;
        }



        //send methods
        public virtual ProcessingResult Send(SignalDispatch<TKey> dispatch)
        {
            return Dispatcher.Send(dispatch).Result;
        }
        public virtual DispatcherAvailability CheckAvailability()
        {
            return Dispatcher.CheckAvailability().Result;
        }


        //limitation methods
        public virtual void CountSendAttempt(SignalDispatch<TKey> dispatch,
            ProcessingResult result, DispatcherAvailability availability)
        {
            if (result == ProcessingResult.Success)
            {
                AvailableLimitCapacity--;
                LimitCounter.InsertTime();
                Interrupter.Success(dispatch);
            }
            else if (result == ProcessingResult.Fail)
            {
                AvailableLimitCapacity--;
                LimitCounter.InsertTime();
                Interrupter.Fail(dispatch, availability);
            }
        }

        public virtual void CountAvailabilityCheck(DispatcherAvailability availability)
        {
            if (availability == DispatcherAvailability.NotChecked)
            {
                return;
            }

            //count check as additional message dispatched
            AvailableLimitCapacity--;
            LimitCounter.InsertTime();
        }

        public virtual void SetLimitsCapacity()
        {
            AvailableLimitCapacity = LimitCounter.GetLimitCapacity();
        }

        public virtual void SetRestrictionsDuration()
        {
            _nextLimitsEndUtc = LimitCounter.GetLimitsEndTimeUtc();
            _nextTimeoutEndUtc = Interrupter.GetTimeoutEndUtc();
        }



        //IDisposable
        public virtual void Dispose()
        {
            if (Dispatcher != null)
                Dispatcher.Dispose();

            if (LimitCounter != null)
                LimitCounter.Dispose();

            if (Interrupter != null)
                Interrupter.Dispose();
        }
    }
}
