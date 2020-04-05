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
    public class StaticTimeoutInterrupter<TKey> : IInterrupter<TKey>
        where TKey : struct
    {
        //fields
        protected int _failedAttemptsCount;
        protected DateTime? _timeoutEndUtc;


        //properties
        /// <summary>
        /// Timeout duration that is applied after number of failed attempts.
        /// </summary>
        public TimeSpan TimeoutDuration { get; set; }
        /// <summary>
        /// Number of failed attempts when reached will trigger a timeout to delivery channel.
        /// </summary>
        public int FailedAttemptsCountTimeoutStart { get; set; }


        //init
        public StaticTimeoutInterrupter()
        {
            TimeoutDuration = NotificationsConstants.STATIC_INTERRUPTER_TIMEOUT_DURATION;
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
            if (availability == DispatcherAvailability.Available)
            {
                return;
            }

            _failedAttemptsCount++;
            SetTimeoutEndUtc();
        }

        protected virtual void SetTimeoutEndUtc()
        {
            int failsOverMinimum = _failedAttemptsCount - FailedAttemptsCountTimeoutStart;
            if (failsOverMinimum < 0)
            {
                _timeoutEndUtc = null;
                return;
            }

            _timeoutEndUtc = DateTime.UtcNow + TimeoutDuration;
        }

        public virtual DateTime? GetTimeoutEndUtc()
        {
            return _timeoutEndUtc;
        }

        public void Dispose()
        {
        }
    }
}
