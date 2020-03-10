using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DispatchHandling.Channels;
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
        protected IMonitor<TKey> _monitor;
        protected ILogger _logger;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IChangeNotifier<SignalDispatch<TKey>> _changeNotifier;
        protected IDispatchChannelRegistry<TKey> _dispatcherRegistry;
        protected ISignalDispatchQueries<TKey> _dispatchQueries;


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
        public DatabaseDispatchProvider(IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> eventSink, SenderSettings senderSettings
            , IDispatchChannelRegistry<TKey> dispatcherRegistry, ISignalDispatchQueries<TKey> dispatchQueries, ILogger logger)
        {
            _dispatchQueue = dispatchQueue;
            _monitor = eventSink;
            _dispatcherRegistry = dispatcherRegistry;
            _dispatchQueries = dispatchQueries;
            _logger = logger;

            QueryPeriod = senderSettings.DatabaseSignalProviderQueryPeriod;
            ItemsQueryCount = senderSettings.DatabaseSignalProviderItemsQueryCount;
            MaxFailedAttempts = senderSettings.DatabaseSignalProviderItemsMaxFailedAttempts;
        }

        public DatabaseDispatchProvider(IDispatchQueue<TKey> dispatchQueues, IMonitor<TKey> eventSink, SenderSettings senderSettings
            , IDispatchChannelRegistry<TKey> dispatcherRegistry, ISignalDispatchQueries<TKey> dispatchQueries
            , ILogger logger, IChangeNotifier<SignalDispatch<TKey>> changeNotifier)
            : this(dispatchQueues, eventSink, senderSettings, dispatcherRegistry, dispatchQueries, logger)
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
            _lastQueryTimeUtc = DateTime.UtcNow;
            _lastQueryKeys = activeDeliveryTypes;
            _isLastQueryMaxItemsReceived = false;

            Stopwatch storageQueryTimer = Stopwatch.StartNew();

            List<SignalDispatch<TKey>> items = null;
            try
            {
                items = _dispatchQueries
                    .Select(ItemsQueryCount, activeDeliveryTypes, MaxFailedAttempts)
                    .Result;
            }
            catch (Exception ex)
            {
                items = new List<SignalDispatch<TKey>>();
                _logger.LogError(ex, SenderInternalMessages.DatabaseDispatchProvider_DatabaseError);
            }

            _monitor.DispatchPersistentStorageQueried(storageQueryTimer.Elapsed, items);
            
            _isLastQueryMaxItemsReceived = items.Count == ItemsQueryCount;
            if (_changeNotifier != null && _isLastQueryMaxItemsReceived == false)
            {
                _changeNotifier.StartMonitor();
            }

            _dispatchQueue.Append(items, true);
        }


    }
}
