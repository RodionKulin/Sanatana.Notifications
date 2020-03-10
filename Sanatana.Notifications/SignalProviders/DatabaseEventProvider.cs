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
    public class DatabaseEventProvider<TKey> : IRegularJob, IDatabaseEventProvider
        where TKey : struct
    {
        //fields
        protected DateTime _lastQueryTimeUtc;
        protected bool _isLastQueryMaxItemsReceived;
        protected IMonitor<TKey> _monitor;
        protected ILogger _logger;
        protected IEventQueue<TKey> _eventQueue;
        protected IChangeNotifier<SignalDispatch<TKey>> _changeNotifier;
        protected ISignalEventQueries<TKey> _eventQueries;


        //properties  
        /// <summary>
        /// Query period to permanent storage to fetch new signals.
        /// </summary>
        public TimeSpan QueryPeriod { get; set; }

        /// <summary>
        /// Number of signals queries from permanent storage on 1 request.
        /// </summary>
        public int ItemsQueryCount { get; set; }

        /// <summary>
        /// Maximum number of failed attempts, after which item won't be fetched from permanent storage.
        /// </summary>
        public int MaxFailedAttempts { get; set; }


        //init
        public DatabaseEventProvider(IEventQueue<TKey> eventQueue, IMonitor<TKey> eventSink
            , SenderSettings senderSettings, ISignalEventQueries<TKey> eventQueries, ILogger logger)
        {
            _eventQueue = eventQueue;
            _monitor = eventSink;
            _eventQueries = eventQueries;
            _logger = logger;

            QueryPeriod = senderSettings.DatabaseSignalProviderQueryPeriod;
            ItemsQueryCount = senderSettings.DatabaseSignalProviderItemsQueryCount;
            MaxFailedAttempts = senderSettings.DatabaseSignalProviderItemsMaxFailedAttempts;
        }

        public DatabaseEventProvider(IEventQueue<TKey> eventQueues, IMonitor<TKey> eventSink, SenderSettings senderSettings
            , ISignalEventQueries<TKey> eventQueries, IChangeNotifier<SignalDispatch<TKey>> changeNotifier, ILogger logger)
            : this(eventQueues, eventSink, senderSettings, eventQueries, logger)
        {
            _changeNotifier = changeNotifier;
        }


        //methods
        public virtual void Tick()
        {
            bool isQueryRequired = CheckIsQueryRequired();
            if (isQueryRequired)
            {
                QueryStorage();
            }
        }

        public virtual void Flush()
        {
        }

        protected virtual bool CheckIsQueryRequired()
        {
            bool isEmpty = _eventQueue.CountQueueItems() == 0;
            if (!isEmpty)
            {
                return false;
            }

            bool storageUpdated = _changeNotifier != null
                && _changeNotifier.HasUpdates;

            DateTime nextQueryTimeUtc = _lastQueryTimeUtc + QueryPeriod;
            bool doScheduledQuery = nextQueryTimeUtc <= DateTime.UtcNow;

            return storageUpdated || doScheduledQuery || _isLastQueryMaxItemsReceived;
        }

        protected virtual void QueryStorage()
        {
            _lastQueryTimeUtc = DateTime.UtcNow;
            _isLastQueryMaxItemsReceived = false;

            Stopwatch storageQueryTimer = Stopwatch.StartNew();

            List<SignalEvent<TKey>> items = null;
            try
            {
                items = _eventQueries
                    .Find(ItemsQueryCount, MaxFailedAttempts)
                    .Result;
            }
            catch (Exception ex)
            {
                items = new List<SignalEvent<TKey>>();
                _logger.LogError(ex, SenderInternalMessages.DatabaseEventProvider_DatabaseError);
            }

            _monitor.EventPersistentStorageQueried(storageQueryTimer.Elapsed, items);
            
            _isLastQueryMaxItemsReceived = items.Count == ItemsQueryCount;
            if (_changeNotifier != null && _isLastQueryMaxItemsReceived == false)
            {
                _changeNotifier.StartMonitor();
            }

            _eventQueue.Append(items, true);
        }

    }
}
