using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
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
        protected IMonitor<TKey> _eventSink;
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
            , SenderSettings senderSettings, ISignalEventQueries<TKey> eventQueries)
        {
            _eventQueue = eventQueue;
            _eventSink = eventSink;
            _eventQueries = eventQueries;

            QueryPeriod = senderSettings.DatabaseSignalProviderQueryPeriod;
            ItemsQueryCount = senderSettings.DatabaseSignalProviderItemsQueryCount;
            MaxFailedAttempts = senderSettings.DatabaseSignalProviderItemsMaxFailedAttempts;
        }

        public DatabaseEventProvider(IEventQueue<TKey> eventQueues, IMonitor<TKey> eventSink, SenderSettings senderSettings
            , ISignalEventQueries<TKey> eventQueries, IChangeNotifier<SignalDispatch<TKey>> changeNotifier)
            : this(eventQueues, eventSink, senderSettings, eventQueries)
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

            bool storageUpdated = _changeNotifier != null
                && _changeNotifier.HasUpdates;

            DateTime nextQueryTimeUtc = _lastQueryTimeUtc + QueryPeriod;
            bool doScheduledQuery = nextQueryTimeUtc <= DateTime.UtcNow;

            return isEmpty &&
                (storageUpdated || doScheduledQuery || _isLastQueryMaxItemsReceived);
        }

        protected virtual void QueryStorage()
        {
            _lastQueryTimeUtc = DateTime.UtcNow;
            _isLastQueryMaxItemsReceived = false;

            Stopwatch storageQueryTimer = Stopwatch.StartNew();

            List<SignalEvent<TKey>> items =
                _eventQueries.Select(ItemsQueryCount, MaxFailedAttempts)
                .Result;

            _eventSink.EventPersistentStorageQueried(storageQueryTimer.Elapsed, items);
            
            _isLastQueryMaxItemsReceived = items.Count == ItemsQueryCount;
            if (_changeNotifier != null && _isLastQueryMaxItemsReceived == false)
            {
                _changeNotifier.StartMonitor();
            }

            _eventQueue.Append(items, true);
        }

    }
}
