using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.Locking;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders
{
    public class DatabaseDispatchProvider<TKey> : IRegularJob, IDatabaseDispatchProvider
        where TKey : struct
    {
        //fields
        protected List<int> _lastQueryKeys;
        protected DateTime _lastQueryTimeUtc;
        protected bool _isLastQueryMaxItemsReceived;
        protected bool _isAllInitiallyLockedSelected;

        //dependencies
        protected IMonitor<TKey> _monitor;
        protected ILogger _logger;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IChangeNotifier<SignalDispatch<TKey>> _changeNotifier;
        protected IDispatchChannelRegistry<TKey> _dispatcherRegistry;
        protected ISignalDispatchQueries<TKey> _dispatchQueries;
        protected ILockTracker<TKey> _lockTracker;
        protected IConsolidationLockTracker<TKey> _consolidationLockTracker;
        protected SenderSettings _senderSettings;


        //properties  
        /// <summary>
        /// Query period to permanent storage to fetch new signals.
        /// </summary>
        public TimeSpan QueryPeriod { get; set; }

        /// <summary>
        /// Number of signals queried from permanent storage on signle request.
        /// </summary>
        public int ItemsQueryCount { get; set; }

        /// <summary>
        /// Maximum number of failed attempts after which item won't be fetched from permanent storage any more.
        /// </summary>
        public int MaxFailedAttempts { get; set; }


        //init
        public DatabaseDispatchProvider(IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> eventSink, SenderSettings senderSettings,
            IDispatchChannelRegistry<TKey> dispatcherRegistry, ISignalDispatchQueries<TKey> dispatchQueries, ILogger logger,
            ILockTracker<TKey> lockTracker, IConsolidationLockTracker<TKey> consolidationLockTracker)
        {
            _dispatchQueue = dispatchQueue;
            _monitor = eventSink;
            _dispatcherRegistry = dispatcherRegistry;
            _dispatchQueries = dispatchQueries;
            _logger = logger;
            _lockTracker = lockTracker;
            _consolidationLockTracker = consolidationLockTracker;
            _senderSettings = senderSettings;

            QueryPeriod = senderSettings.DatabaseSignalProviderQueryPeriod;
            ItemsQueryCount = senderSettings.DatabaseSignalProviderItemsQueryCount;
            MaxFailedAttempts = senderSettings.DatabaseSignalProviderItemsMaxFailedAttempts;
        }

        public DatabaseDispatchProvider(IDispatchQueue<TKey> dispatchQueues, IMonitor<TKey> eventSink, SenderSettings senderSettings,
            IDispatchChannelRegistry<TKey> dispatcherRegistry, ISignalDispatchQueries<TKey> dispatchQueries,
            ILogger logger, IChangeNotifier<SignalDispatch<TKey>> changeNotifier,
            ILockTracker<TKey> lockTracker, IConsolidationLockTracker<TKey> consolidationLockTracker)
            : this(dispatchQueues, eventSink, senderSettings, dispatcherRegistry, dispatchQueries, logger, lockTracker, consolidationLockTracker)
        {
            _changeNotifier = changeNotifier;
        }


        //methods
        public virtual void Tick()
        {
            List<int> activeDeliveryTypes = _dispatcherRegistry.GetActiveDeliveryTypes(false);

            bool isQueryRequired = CheckIsQueryRequired(activeDeliveryTypes);
            if (isQueryRequired)
            {
                QueryStorage(activeDeliveryTypes);
            }
        }

        public virtual void Flush()
        {

        }

        protected virtual bool CheckIsQueryRequired(List<int> activeDeliveryTypes)
        {
            bool isEmpty = _dispatchQueue.CheckIsEmpty(activeDeliveryTypes);
            if (!isEmpty)
            {
                return false;
            }

            bool storageUpdated = _changeNotifier != null && _changeNotifier.HasUpdates;

            DateTime nextQueryTimeUtc = _lastQueryTimeUtc + QueryPeriod;
            bool doScheduledQuery = nextQueryTimeUtc <= DateTime.UtcNow;

            bool hasUnqueriedKeys = _lastQueryKeys == null
               || activeDeliveryTypes.Except(_lastQueryKeys).Count() > 0;

            return storageUpdated || doScheduledQuery || hasUnqueriedKeys || _isLastQueryMaxItemsReceived;
        }

        protected virtual void QueryStorage(List<int> activeDeliveryTypes)
        {
            //update query state
            _lastQueryTimeUtc = DateTime.UtcNow;
            _lastQueryKeys = activeDeliveryTypes;
            _isLastQueryMaxItemsReceived = false;

            //query items
            Stopwatch storageQueryTimer = Stopwatch.StartNew();
            DateTime lockStartUtc = DateTime.UtcNow;

            List<SignalDispatch<TKey>> items = null;
            try
            {
                items = PickStorageQuery(activeDeliveryTypes);
            }
            catch (Exception ex)
            {
                items = new List<SignalDispatch<TKey>>();
                _logger.LogError(ex, SenderInternalMessages.DatabaseDispatchProvider_DatabaseError);
            }
            storageQueryTimer.Stop();

            //enqueue items for processing
            _dispatchQueue.Append(items, true);

            //turn on database event tracking
            _isLastQueryMaxItemsReceived = items.Count == ItemsQueryCount;
            if (_changeNotifier != null && _isLastQueryMaxItemsReceived == false)
            {
                _changeNotifier.StartMonitor();
            }

            //store metrics
            _monitor.DispatchPersistentStorageQueried(storageQueryTimer.Elapsed, items);

            //remember processed items, not to query them again while processing
            _lockTracker.RememberLock(items.Select(x => x.SignalDispatchId), lockStartUtc);
        }

        protected virtual List<SignalDispatch<TKey>> PickStorageQuery(List<int> activeDeliveryTypes)
        {
            DateTime lockExpirationDate = _lockTracker.GetMaxExpiredLockSinceUtc();
            var queryParams = new DAL.Parameters.DispatchQueryParameters<TKey>
            {
                Count = ItemsQueryCount,
                ActiveDeliveryTypes = activeDeliveryTypes,
                MaxFailedAttempts = MaxFailedAttempts,
                ExcludeIds = _lockTracker.GetLockedIds(),
                ExcludeConsolidated = _consolidationLockTracker.GetLockedGroups()
            };

            if (_isAllInitiallyLockedSelected == false)
            {
                //if instance was terminated and did not release lock, then process already locked items first.
                List<SignalDispatch<TKey>> lockedItems = _dispatchQueries.SelectLocked(queryParams, _senderSettings.LockedByInstanceId.Value, lockExpirationDate)
                    .Result;
                _isAllInitiallyLockedSelected = lockedItems.Count < ItemsQueryCount;
                if(lockedItems.Count > 0)
                {
                    //if locked items are present then process them,
                    //else make query for next unlocked items.
                    return lockedItems;
                }
            }

            bool isLockingEnabled = _senderSettings.IsDbLockStorageEnabled;
            if (isLockingEnabled)
            {
                return _dispatchQueries
                    .SelectWithSetLock(queryParams, _senderSettings.LockedByInstanceId.Value, lockExpirationDate)
                    .Result;
            }

            //select without locking in database
            return _dispatchQueries.SelectNotSetLock(queryParams).Result;
        }
    }
}
