using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender
{
    internal static class SenderConstants
    {
        //Dispatcher
        public static readonly TimeSpan DISPATCHER_TIMER_INTERVAL = TimeSpan.FromMilliseconds(100);


        //MessageProvider
        public const int STORAGE_MESSAGE_QUERY_COUNT_DEFAULT = 100;
        public static readonly TimeSpan STORAGE_QUERY_PERIOD_DEFAULT = TimeSpan.FromSeconds(60);


        //MessageQueue
        public const int MAX_DELIVERY_FAILED_ATTEMPTS_DEFAULT = 2;
        public static readonly TimeSpan FAILED_ATTEMPT_RETRY_PERIOD_DEFAULT = TimeSpan.FromMinutes(10);


        //ProgressiveFailPenalty
        /// <summary>
        /// Начальная пауза при достижении лимита неудавшихся доставок.
        /// </summary>
        public static readonly TimeSpan FAILED_PENALTY_START_TIME_DEFAULT = TimeSpan.FromSeconds(10);
        /// <summary>
        /// Пауза при достижении лимита неудавшихся доставок.
        /// </summary>
        public static readonly TimeSpan FAILED_PENALTY_MAX_TIME_DEFAULT = TimeSpan.FromMinutes(5);
        /// <summary>
        /// Лимит неудавшихся доставок, при достижении которого делается пауза.
        /// </summary>
        public const int FAILED_PENALTY_MINIMUM_ATTEMPT_DEFAULT = 3;


        //PeriodLimitManager
        public static readonly TimeSpan JOURNAL_STORAGE_CLEAN_PERIOD = TimeSpan.FromMinutes(5);
        public const int JOURNAL_STORAGE_CLEAN_INSERT_COUNT = 100;
    }
}
