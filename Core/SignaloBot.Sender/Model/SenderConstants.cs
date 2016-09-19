using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender
{
    internal static class SenderConstants
    {
        //hub
        public static readonly TimeSpan QUEUES_TICK_INTERVAL = TimeSpan.FromMilliseconds(100);
        public static readonly TimeSpan PROCESSORS_TICK_INTERVAL = TimeSpan.FromMilliseconds(20);
        public static readonly TimeSpan STOPPING_MONITOR_TICK_INTERVAL = TimeSpan.FromMilliseconds(100);

        //Queue
        public const int QUEUE_ITEMS_QUERY_COUNT = 500;
        public static readonly TimeSpan QUEUE_QUERY_PERIOD = TimeSpan.FromSeconds(60);
        public const int QUEUE_MAX_FAILED_ATTEMPTS = 2;
        public static readonly TimeSpan QUEUE_FAILED_ATTEMPT_RETRY_PERIOD = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan QUEUE_FLUSH_PERIOD = TimeSpan.FromSeconds(20);
        public const int QUEUE_RETURN_TO_STORAGE_AFTER_ITEMS_COUNT = 1000;


        //Composer
        public const int COMPOSER_SUBSCRIBERS_QUERY_COUNT = 2000;



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
