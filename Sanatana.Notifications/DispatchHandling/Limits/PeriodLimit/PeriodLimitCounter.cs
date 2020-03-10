using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sanatana.Notifications.DispatchHandling.Limits.JournalStorage;

namespace Sanatana.Notifications.DispatchHandling.Limits
{
    /// <summary>
    /// LimitCounter that manages limits based on amounts of signals processed per timespan.
    /// </summary>
    [Serializable]
    public class PeriodLimitCounter : ILimitCounter
    {
        //fields
        protected List<LimitedPeriod> _limitedPeriods;
        /// <summary>
        /// Journal with dispatch timestamps. Is always ordered ascending.
        /// </summary>
        protected IJournalStorage _journalStorage;
        protected object _journalLock;
        protected DateTime _lastJournalCleanUtc;
        protected int _newInsertsAfterClean;


        //properties
        /// <summary>
        /// Period to clean journal entries that fall out of scope of monitoring timespans.
        /// </summary>
        public TimeSpan JournalCleanPeriod { get; set; }
        /// <summary>
        /// Max number of new journal entries from previous journal cleaning to trigger journal cleaning again.
        /// </summary>
        public int JournalCleanAfterInsertsCount { get; set; }


        //init
        public PeriodLimitCounter(List<LimitedPeriod> limitedPeriods, IJournalStorage journalStorage)
        {
            _limitedPeriods = new List<LimitedPeriod>(limitedPeriods);
            _journalStorage = journalStorage;

            _journalLock = new object();
            _lastJournalCleanUtc = DateTime.MinValue;
            _newInsertsAfterClean = 0;

            JournalCleanPeriod = NotificationsConstants.JOURNAL_STORAGE_CLEAN_PERIOD;
            JournalCleanAfterInsertsCount = NotificationsConstants.JOURNAL_STORAGE_CLEAN_INSERT_COUNT;
        }


        //methods
        public virtual void InsertTime()
        {
            lock (_journalLock)
            {
                _journalStorage.InsertTime();
                _newInsertsAfterClean++;
                CleanJournal();
            }
        }

        public virtual int GetLimitCapacity()
        {
            int maxSendingCapacity;

            lock (_journalLock)
            {
                maxSendingCapacity = _journalStorage.GetSendingCapacity(_limitedPeriods);
                CleanJournal();
            }

            return maxSendingCapacity;
        }

        public virtual DateTime? GetLimitsEndTimeUtc()
        {
            DateTime? limitsEndTimeUtc;

            lock (_journalLock)
            {
                limitsEndTimeUtc = _journalStorage.GetLimitsEndTimeUtc(_limitedPeriods);
                CleanJournal();
            }

            return limitsEndTimeUtc;
        }

        protected virtual void CleanJournal()
        {
            TimeSpan periodFromLastClean = DateTime.UtcNow - _lastJournalCleanUtc;

            TimeSpan journalCleanPeriod = JournalCleanPeriod <= TimeSpan.Zero
                ? NotificationsConstants.JOURNAL_STORAGE_CLEAN_PERIOD
                : JournalCleanPeriod;

            int journalCleanAfterInsertsCount = JournalCleanAfterInsertsCount <= 0
                ? NotificationsConstants.JOURNAL_STORAGE_CLEAN_INSERT_COUNT
                : JournalCleanAfterInsertsCount;

            if (periodFromLastClean > journalCleanPeriod
                || _newInsertsAfterClean > journalCleanAfterInsertsCount)
            {
                TimeSpan deleteBeforePeriod = _limitedPeriods.Max(p => p.Period);
                _journalStorage.CleanJournal(deleteBeforePeriod);

                _lastJournalCleanUtc = DateTime.UtcNow;
                _newInsertsAfterClean = 0;
            }
        }


        //dispose
        public virtual void Dispose()
        {
            _journalStorage.Dispose();
        }
    }
}