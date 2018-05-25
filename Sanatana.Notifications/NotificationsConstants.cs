using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications
{
    internal static class NotificationsConstants
    {
        //RegularJobs
        public static readonly TimeSpan REGULAR_JOB_TICK_INTERVAL = TimeSpan.FromMilliseconds(100);


        //DatabaseSignalsProviders
        public const int DATABASE_SIGNAL_PROVIDER_ITEMS_QUERY_COUNT = 100;
        public static readonly TimeSpan DATABASE_SIGNAL_PROVIDER_QUERY_PERIOD = TimeSpan.FromSeconds(60);
        public const int DATABASE_SIGNAL_PROVIDER_MAX_FAILED_ATTEMPTS = 2;


        //Queues
        public static readonly TimeSpan SIGNAL_QUEUE_ON_FAILED_ATTEMPT_RETRY_PERIOD = TimeSpan.FromMinutes(5);
        public static bool SIGNAL_QUEUE_IS_TEMPORARY_STORAGE_ENABLED = true;
        public const int SIGNAL_QUEUE_PERSIST_BEGIN_ON_ITEMS_COUNT = 1000;
        public const int QUEUE_TARGET_PERSIST_END_ON_ITEMS_COUNT = 200;


        //FlushJobs
        public static readonly TimeSpan FLUSH_JOB_FLUSH_PERIOD = TimeSpan.FromSeconds(2);
        public const int FLUSH_JOB_QUEUE_LIMIT = 100;


        //Temporary storage
        public static readonly Version TS_ENTITIES_VERSION = new Version(1, 0, 0, 0);
        public const string TS_EVENT_QUEUE_KEY = "E";
        public const string TS_DISPATCH_QUEUE_KEY = "D";


        //Composer
        public const int SUBSCRIBERS_FETCHER_ITEMS_QUERY_LIMIT = 1000;


        //Interrupters
        /// <summary>
        /// Timeout duration that is applied after number of failed attempts.
        /// </summary>
        public static readonly TimeSpan STATIC_INTERRUPTER_TIMEOUT_DURATION = TimeSpan.FromMinutes(1);
        /// <summary>
        /// Starting timeout duration that is applied after number of failed attempts first time.
        /// </summary>
        public static readonly TimeSpan PROGRESSIVE_INTERRUPTER_TIMEOUT_MIN_DURATION = TimeSpan.FromSeconds(10);
        /// <summary>
        /// Max timeout duration that can be applied.
        /// </summary>
        public static readonly TimeSpan PROGRESSIVE_INTERRUPTER_TIMEOUT_MAX_DURATION = TimeSpan.FromMinutes(5);
        /// <summary>
        /// Number of failed attempts when reached will trigger a timeout to delivery channel.
        /// </summary>
        public const int FAILED_ATTEMPTS_COUNT_TIMEOUT_START = 3;


        //PeriodLimitManager
        /// <summary>
        /// Period to clean journal entries that fall out of scope of monitoring timespans.
        /// </summary>
        public static readonly TimeSpan JOURNAL_STORAGE_CLEAN_PERIOD = TimeSpan.FromMinutes(5);
        /// <summary>
        /// Max number of new journal entries from previous journal cleaning to trigger journal cleaning again.
        /// </summary>
        public const int JOURNAL_STORAGE_CLEAN_INSERT_COUNT = 100;


        //Emails
        /// <summary>
        /// Maximum address length accoring to http://www.rfc-editor.org/errata_search.php?rfc=3696&eid=1690 and https://stackoverflow.com/questions/386294/what-is-the-maximum-length-of-a-valId-email-address
        /// </summary>
        public const int EMAIL_MAX_ADDRESS_LENGTH = 254;
        public const int EMAIL_MAX_SUBJECT_LENGTH = 200;
        public const int EMAIL_MAX_SENDER_NAME_LENGTH = 200;
        public const int EMAIL_MAX_CONTENT_LENGTH = 20000000;
    }
}
