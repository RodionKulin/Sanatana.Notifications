using Sanatana.Notifications.Locking;
using Sanatana.Notifications.Models;
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
        protected TimeSpan _lockDuration = NotificationsConstants.DATABASE_LOCK_DURATION;



        //Queues
        /// <summary>
        /// Pause duration after failed attempt or dispatcher not available before retrying.
        /// </summary>
        public TimeSpan SignalQueueRetryPeriod { get; set; } = NotificationsConstants.SIGNAL_QUEUE_ON_FAILED_ATTEMPT_RETRY_PERIOD;
        /// <summary>
        /// Enable storing items in temporary storage while they are processed to prevent data loss in case of power down.
        /// </summary>
        public bool SignalQueueIsTemporaryStorageEnabled { get; set; } = NotificationsConstants.SIGNAL_QUEUE_IS_TEMPORARY_STORAGE_ENABLED;
        /// <summary>
        /// Start flushing queue items to permanent storage after exceeding this limit.
        /// </summary>
        public int SignalQueuePersistBeginOnItemsCount { get; set; } = NotificationsConstants.SIGNAL_QUEUE_PERSIST_BEGIN_ON_ITEMS_COUNT;
        /// <summary>
        /// Will flush to permanent storage until reached SignalQueuePersistEndOnItemsCount number.
        /// Total number of items to be flushed is SignalQueuePersistBeginOnItemsCount minus SignalQueuePersistEndOnItemsCount.
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
        /// <summary>
        /// Wait during FlushJobFlushPeriod to accumulate batch of notifications to flush into database.
        /// </summary>
        public bool FlushJobBatchingEnabled { get; set; } = NotificationsConstants.FLUSH_JOB_BATCHING_ENABLED;


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
        /// <summary>
        /// Number of subscribers selected in a single batch
        /// </summary>
        public int SubscribersFetcherItemsQueryLimit { get; set; } = NotificationsConstants.SUBSCRIBERS_FETCHER_ITEMS_QUERY_LIMIT;


        //WcfSignalProvider
        /// <summary>
        /// Register derived types that should be accepted by WcfSignalProvider
        /// </summary>
        public static List<Type> KnownWCFServiceTypes { get; set; }


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
        /// <summary>
        /// Release lock period when SignalDispatch becomes available for processing again after unreleased lock.
        /// </summary>
        public virtual TimeSpan LockDuration
        {
            get
            {
                return _lockDuration;
            }
            set
            {
                TimeSpan min = NotificationsConstants.DATABASE_MIN_LOCK_DURATION;
                if (_lockDuration <= min)
                {
                    string message = string.Format(SenderInternalMessages.SenderSettings_MinLockDurationException,
                        min, nameof(SenderSettings.LockDuration), "_expireBeforehandInterval", typeof(LockTracker<>).FullName);
                    throw new NotSupportedException(message);
                }
                _lockDuration = value;
            }
        }
        
        /// <summary>
        /// Lock Id that should be unique for each instance of Sender. 
        /// Null value will disable locking in database.
        /// Is used for Signal locks and ConsolidatinoLocks
        /// </summary>
        public Guid? LockedByInstanceId { get; set; }

        /// <summary>
        /// Show if lock property be updated when selecting Signals and from database, so they are not processed multiple times by different Sender instances.
        /// Shows status of individual Signals locks enabled and consolidation locks enabled.
        /// To enable assign not null value to DatabaseSignalLockId.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDbLockStorageEnabled
        {
            get
            {
                return LockedByInstanceId != null;
            }
        }


        //Other network SignalProviders
        /// <summary>
        /// Default write concern for network listening SignalProviders. 
        /// If SignalWriteConcern is not provided by caller, then SenderSettings.DefaultWriteConcern is used.
        /// If SenderSettings.DefaultWriteConcern is not specified, then MemoryOnly is used.
        /// TemporaryStorage is only set globally by SignalQueueIsTemporaryStorageEnabled and can is not controlled for individual Signal.
        /// </summary>
        public SignalWriteConcern DefaultWriteConcern { get; set; }


        //init
        public SenderSettings()
        {
            MaxParallelDispatchesProcessed = Environment.ProcessorCount * 2;
            MaxParallelEventsProcessed = Environment.ProcessorCount * 2;
        }


        //methods
        public static IEnumerable<Type> GetKnownServiceTypes(ICustomAttributeProvider provider)
        {
            return KnownWCFServiceTypes ?? new List<Type>();
        }

        public virtual SignalWriteConcern GetWriteConcernOrDefault(SignalWriteConcern writeConcern)
        {
            if(writeConcern != SignalWriteConcern.Default)
            {
                return writeConcern;
            }

            if (DefaultWriteConcern != SignalWriteConcern.Default)
            {
                return DefaultWriteConcern;
            }

            return SignalWriteConcern.MemoryOnly;
        }
    }
}
