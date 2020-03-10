using Sanatana.Notifications.Processing;
using Sanatana.Notifications.DispatchHandling.Interrupters;
using Sanatana.Notifications.DispatchHandling.Limits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DispatchHandling.DeliveryTypes;

namespace Sanatana.Notifications.DispatchHandling.Channels
{
    public class DispatchChannel<TKey> : IDisposable
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

        

        //methods
        public virtual DispatcherAvailability ApplyResult(ProcessingResult result, SignalDispatch<TKey> dispatch, ILogger logger)
        {
            if(result == ProcessingResult.Success)
            {
                AvailableLimitCapacity--;
                LimitCounter.InsertTime();
                Interrupter.Success(dispatch);
                return DispatcherAvailability.Available;
            }
            else if (result == ProcessingResult.Fail)
            {
                DispatcherAvailability availability = CheckAvailability(logger);                

                AvailableLimitCapacity--;
                LimitCounter.InsertTime();
                Interrupter.Fail(dispatch, availability);

                return availability;
            }
            else
            {
                return DispatcherAvailability.NotChecked;
            }
        }
        
        protected virtual DispatcherAvailability CheckAvailability(ILogger logger)
        {
            DispatcherAvailability availability;
            try
            {
                availability = Dispatcher.CheckAvailability().Result;
            }
            catch (Exception ex)
            {
                availability = DispatcherAvailability.NotAvailable;
                logger.LogError(ex, null);
            }
            
            //count check as additional message dispatched
            if (availability != DispatcherAvailability.NotChecked)
            {
                AvailableLimitCapacity--;
                LimitCounter.InsertTime();
            }

            return availability;
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
            if(Dispatcher != null)
                Dispatcher.Dispose();

            if(LimitCounter != null)
                LimitCounter.Dispose();

            if (Interrupter != null)
                LimitCounter.Dispose();
        }
    }
}
