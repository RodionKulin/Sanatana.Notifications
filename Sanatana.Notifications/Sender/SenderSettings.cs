using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Sender
{
    public class SenderSettings
    {
        //fields
        protected int _maxParallelDispatchesProcessed;
        protected int _maxParallelEventsProcessed;



        //DatabaseSignalsProviders
        /// <summary>
        /// Number of signals queried from permanent storage on signle request.
        /// </summary>
        public int DatabaseSignalProviderItemsQueryCount { get; set; } = NotificationsConstants.DATABASE_SIGNAL_PROVIDER_ITEMS_QUERY_COUNT;
        /// <summary>
        /// Maximum number of failed attempts after which item won't be fetched from permanent storage any more.
        /// </summary>
        public int DatabaseSignalProviderItemsMaxFailedAttempts { get; set; } = NotificationsConstants.DATABASE_SIGNAL_PROVIDER_MAX_FAILED_ATTEMPTS;
        /// <summary>
        /// Query period to permanent storage to fetch new signals.
        /// </summary>
        public TimeSpan DatabaseSignalProviderQueryPeriod { get; set; } = NotificationsConstants.DATABASE_SIGNAL_PROVIDER_QUERY_PERIOD;


        //Queues
        /// <summary>
        /// Pause duration after failed attempt before retrying.
        /// </summary>
        public TimeSpan SignalQueueOnFailedAttemptRetryPeriod { get; set; } = NotificationsConstants.SIGNAL_QUEUE_ON_FAILED_ATTEMPT_RETRY_PERIOD;
        /// <summary>
        /// Enable storing items in temporary storage while they are processed to prevent data loss in case of power down.
        /// </summary>
        public bool SignalQueueIsTemporaryStorageEnabled { get; set; } = NotificationsConstants.SIGNAL_QUEUE_IS_TEMPORARY_STORAGE_ENABLED;
        /// <summary>
        /// Items number limit when exceeded starts flushing queue items to permanent storage.
        /// </summary>
        public int SignalQueuePersistBeginOnItemsCount { get; set; } = NotificationsConstants.SIGNAL_QUEUE_PERSIST_BEGIN_ON_ITEMS_COUNT;
        /// <summary>
        /// Target number of queue items when when flishing to permanent storage stops. 
        /// </summary>
        public int SignalQueuePersistEndOnItemsCount { get; set; } = NotificationsConstants.QUEUE_TARGET_PERSIST_END_ON_ITEMS_COUNT;


        //FlushJobs
        /// <summary>
        /// Max time until next flush of SignalEvent and SignalDispatch batch of Insert, Update, Delete operations.
        /// </summary>
        public TimeSpan FlushJobFlushPeriod { get; set; } = NotificationsConstants.FLUSH_JOB_FLUSH_PERIOD;
        /// <summary>
        /// Number of items in flush queue when reached will trigger flush of SignalEvent and SignalDispatch batch of Insert, Update, Delete operations.
        /// </summary>
        public int FlushJobQueueLimit { get; set; } = NotificationsConstants.FLUSH_JOB_QUEUE_LIMIT;


        //Processors
        /// <summary>
        /// Maximum number of parallel dispatches processed to be sent. By default equal to processors count.
        /// </summary>
        public int MaxParallelDispatchesProcessed
        {
            get
            {
                return _maxParallelDispatchesProcessed;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(SenderInternalMessages.ProcessorBase_ParallelNumberOutOfRange);
                }

                _maxParallelDispatchesProcessed = value;
            }
        } 
        /// <summary>
        /// Maximum number of parallel events processed to compose dispatches. By default equal to processors count.
        /// </summary>
        public int MaxParallelEventsProcessed
        {
            get
            {
                return _maxParallelEventsProcessed;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(SenderInternalMessages.ProcessorBase_ParallelNumberOutOfRange);
                }

                _maxParallelEventsProcessed = value;
            }
        }


        //SubscribersFetcher
        public int SubscribersFetcherItemsQueryLimit { get; set; } = NotificationsConstants.SUBSCRIBERS_FETCHER_ITEMS_QUERY_LIMIT;


        //WcfSignalProvider
        /// <summary>
        /// Register derived types that should be accepted by WcfSignalProvider
        /// </summary>
        public static List<Type> KnownWCFServiceTypes { get; set; }


        //init
        public SenderSettings()
        {
            MaxParallelDispatchesProcessed = Environment.ProcessorCount;
            MaxParallelEventsProcessed = Environment.ProcessorCount;
        }


        //methods
        public static IEnumerable<Type> GetKnownServiceTypes(ICustomAttributeProvider provider)
        {
            return KnownWCFServiceTypes ?? new List<Type>();
        }
    }
}
