using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Dispatching.Limits.JournalStorage
{
    public class MemoryJournalStorage : IJournalStorage
    {
        //fields
        protected List<DateTime> _journal;


        //init
        public MemoryJournalStorage()
        {
            _journal = new List<DateTime>();
        }


        //IJournalStorage
        public virtual void InsertTime()
        {
            _journal.Add(DateTime.UtcNow);
        }

        public virtual int GetSendingCapacity(List<LimitedPeriod> periods)
        {
            int minSendingCapacity = int.MaxValue;

            foreach (LimitedPeriod period in periods)
            {
                DateTime countFrom = DateTime.UtcNow - period.Period;

                int journaledForPeriod = _journal.Count(p => p > countFrom);
                int sendingCapacity = period.Limit - journaledForPeriod;

                if (sendingCapacity < minSendingCapacity)
                {
                    minSendingCapacity = sendingCapacity;
                }
            }

            return minSendingCapacity;
        }

        public virtual DateTime? GetLimitsEndTimeUtc(List<LimitedPeriod> periods)
        {
            if (periods.Count == 0 || _journal.Count == 0)
            {
                return null;
            }

            DateTime maxLimitEndTimeUtc = DateTime.UtcNow;

            foreach (LimitedPeriod limitedPeriod in periods)
            {
                DateTime limitPeriodBeginTime = DateTime.UtcNow - limitedPeriod.Period;
                int indexOfFirstItemInPeriod = _journal.FindIndex(p => p > limitPeriodBeginTime);
                
                //if amount of records in limit period in aloowed, then no pausing is required
                int countInLimitedPeriod = _journal.Count - indexOfFirstItemInPeriod;
                bool isSendingAvailable = limitedPeriod.Limit > countInLimitedPeriod;
                if (isSendingAvailable)
                    continue;

                //find first timespamp in journal matching limit amount
                int indexOfFirstItemInLimit = _journal.Count - limitedPeriod.Limit;
                DateTime firstItemInLimit = _journal[indexOfFirstItemInLimit];

                //determine pause end time
                DateTime limitEndTimeUtc = firstItemInLimit + limitedPeriod.Period;
                if (limitEndTimeUtc > maxLimitEndTimeUtc)
                    maxLimitEndTimeUtc = limitEndTimeUtc;
            }

            return maxLimitEndTimeUtc;
        }

        public virtual void CleanJournal(TimeSpan deleteBeforePeriod)
        {
            DateTime deleteBeforeDateUtc = DateTime.UtcNow - deleteBeforePeriod;

            while (_journal.Count > 0 && _journal[0] < deleteBeforeDateUtc)
            {
                _journal.RemoveAt(0);
            }
        }
        


        //disposable
        public virtual void Dispose()
        {

        }
    }
}
