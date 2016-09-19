using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.Sender.Senders.FailCounter
{
    public class ProgressiveFailCounter : IFailCounter
    {
        //состояние
        protected int _failedAttemptCount;
        protected DateTime? _lastFailedAttemptTimeUtc;
        protected DateTime? _penaltyEndTime;


        //настройки
        public TimeSpan FailedPenaltyStartTime { get; set; }
        public TimeSpan FailedPenaltyMaxTime { get; set; }
        public int FailedPenaltyMinimumAttempt { get; set; }


        //зависимые свойства
        public virtual bool IsPenaltyActive
        {
            get
            {
                return _penaltyEndTime != null && _penaltyEndTime > DateTime.UtcNow;
            }
        }


        //инициализация
        public ProgressiveFailCounter()
        {
            FailedPenaltyStartTime = SenderConstants.FAILED_PENALTY_START_TIME_DEFAULT;
            FailedPenaltyMaxTime = SenderConstants.FAILED_PENALTY_MAX_TIME_DEFAULT;
            FailedPenaltyMinimumAttempt = SenderConstants.FAILED_PENALTY_MINIMUM_ATTEMPT_DEFAULT;
        }



        //методы
        public virtual void Fail()
        {
            _failedAttemptCount++;
            _lastFailedAttemptTimeUtc = DateTime.UtcNow;
            SetPenaltyEndTime(_lastFailedAttemptTimeUtc.Value);
        }

        public virtual void Success()
        {
            _failedAttemptCount = 0;
            _lastFailedAttemptTimeUtc = null;
            _penaltyEndTime = null;
        }

        protected virtual void SetPenaltyEndTime(DateTime lastFailAttemptTimeUtc)
        {
            int failsOverMinimum = _failedAttemptCount - FailedPenaltyMinimumAttempt;
            if(failsOverMinimum < 0)
            {
                _penaltyEndTime = null;
                return;
            }

            double penaltyMultiplier = Math.Pow(2, failsOverMinimum);
            TimeSpan actualPenalty = FailedPenaltyStartTime.Multiply(penaltyMultiplier);

            if (actualPenalty > FailedPenaltyMaxTime)
            {
                actualPenalty = FailedPenaltyMaxTime;
            }

            _penaltyEndTime = lastFailAttemptTimeUtc + actualPenalty;
        }
    }
}
