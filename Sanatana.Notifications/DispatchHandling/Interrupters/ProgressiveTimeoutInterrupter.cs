using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DispatchHandling.Interrupters
{
    /// <summary>
    /// Increase timeout time on each failed delivery. Multiply minimum timeout duration to the fails count power of 2 until timeout maximum duration is reached.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class ProgressiveTimeoutInterrupter<TKey> : IInterrupter<TKey>
        where TKey : struct
    {
        //fields
        protected int _failedAttemptsCount;
        protected DateTime? _timeoutEndUtc;


        //settings
        /// <summary>
        /// Starting timeout duration that is applied after number of failed attempts first time.
        /// </summary>
        public TimeSpan TimeoutMinDuration { get; set; }
        /// <summary>
        /// Max timeout duration that can be applied.
        /// </summary>
        public TimeSpan TimeoutMaxDuration { get; set; }
        /// <summary>
        /// Number of failed attempts when reached will trigger a timeout to delivery channel.
        /// </summary>
        public int FailedAttemptsCountTimeoutStart { get; set; }


        //init
        public ProgressiveTimeoutInterrupter()
        {
            TimeoutMinDuration = NotificationsConstants.PROGRESSIVE_INTERRUPTER_TIMEOUT_MIN_DURATION;
            TimeoutMaxDuration = NotificationsConstants.PROGRESSIVE_INTERRUPTER_TIMEOUT_MAX_DURATION;
            FailedAttemptsCountTimeoutStart = NotificationsConstants.FAILED_ATTEMPTS_COUNT_TIMEOUT_START;
        }



        //methods
        public virtual void Success(SignalDispatch<TKey> dispatch)
        {
            _failedAttemptsCount = 0;
            _timeoutEndUtc = null;
        }

        public virtual void Fail(SignalDispatch<TKey> dispatch, DispatcherAvailability availability)
        {
            if(availability == DispatcherAvailability.Available)
            {
                return;
            }

            _failedAttemptsCount++;
            SetTimeoutEndUtc();
        }

        protected virtual void SetTimeoutEndUtc()
        {
            int failsOverMinimum = _failedAttemptsCount - FailedAttemptsCountTimeoutStart;
            if(failsOverMinimum < 0)
            {
                _timeoutEndUtc = null;
                return;
            }

            int penaltyMultiplier = (int)Math.Pow(2, failsOverMinimum);         
            TimeSpan actualPenalty = TimeSpan.FromTicks(TimeoutMinDuration.Ticks * penaltyMultiplier);
            
            if (actualPenalty > TimeoutMaxDuration
                || actualPenalty < TimeSpan.Zero)   //prevent value overflow
            {
                actualPenalty = TimeoutMaxDuration;
            }

            _timeoutEndUtc = DateTime.UtcNow + actualPenalty;
        }

        public virtual DateTime? GetTimeoutEndUtc()
        {
            return _timeoutEndUtc;
        }
    }
}
