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
using Common.Utility;
using SignaloBot.Sender.Senders.LimitManager.JournalStorage;

namespace SignaloBot.Sender.Senders.LimitManager
{
    /// <summary>
    /// Управлением лимитами, основанными на количестве за период.
    /// </summary>
    [Serializable]
    public class PeriodLimitManager : ILimitManager
    {
        //поля
        protected List<LimitedPeriod> _limitedPeriods;
        /// <summary>
        /// Журнал с временными метками об отправлении. Всегда упорядочен от меньшего к большему значению.
        /// </summary>
        protected IJournalStorage _journalStorage;
        protected object _journalLock;
        protected DateTime _lastJournalCleanUtc;
        protected int _newInsertsAfterClean;


        //свойства
        public TimeSpan JournalCleanPeriod { get; set; }
        public int JournalCleanAfterInsertsCount { get; set; }


        //инициализация
        public PeriodLimitManager(List<LimitedPeriod> limitedPeriods, IJournalStorage journalStorage)
        {
            _limitedPeriods = new List<LimitedPeriod>(limitedPeriods);
            _journalStorage = journalStorage;

            _journalLock = new object();
            _lastJournalCleanUtc = DateTime.MinValue;
            _newInsertsAfterClean = 0;

            JournalCleanPeriod = SenderConstants.JOURNAL_STORAGE_CLEAN_PERIOD;
            JournalCleanAfterInsertsCount = SenderConstants.JOURNAL_STORAGE_CLEAN_INSERT_COUNT;
        }

                

        //Добавить время отправки
        /// <summary>
        /// Добавить метку о доставке в журнал
        /// </summary>
        /// <returns></returns>
        public virtual void InsertTime()
        {
            lock (_journalLock)
            {
                _journalStorage.InsertTime();
                _newInsertsAfterClean++;
                CleanJournal();
            }
        }
        
        

        //Проверить доступность
        /// <summary>
        /// Получить доступное количество действий до достижения лимита.
        /// В случае ошибки возвращается 0.
        /// </summary>
        /// <returns></returns>
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
        
        /// <summary>
        /// Получить время до снятия лимитов
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetLimitsEndTimeUtc()
        {
            DateTime limitsEndTimeUtc;

            lock (_journalLock)
            {
                limitsEndTimeUtc = _journalStorage.GetLimitsEndTimeUtc(_limitedPeriods);
                CleanJournal();
            }

            return limitsEndTimeUtc;
        }
              


        //Очистить старые записи
        protected virtual void CleanJournal()
        {
            TimeSpan periodFromLastClean = DateTime.UtcNow - _lastJournalCleanUtc;

            TimeSpan journalCleanPeriod = JournalCleanPeriod <= TimeSpan.Zero
                ? SenderConstants.JOURNAL_STORAGE_CLEAN_PERIOD
                : JournalCleanPeriod;

            int journalCleanAfterInsertsCount = JournalCleanAfterInsertsCount <= 0
                ? SenderConstants.JOURNAL_STORAGE_CLEAN_INSERT_COUNT
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


        //IDisposable
        public virtual void Dispose()
        {
            _journalStorage.Dispose();
        }
    }
}