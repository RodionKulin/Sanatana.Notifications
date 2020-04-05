using Sanatana.Notifications;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders
{
    public abstract class BaseSignalProvider<TKey>
        where TKey : struct
    {
        //fields
        protected IEventQueue<TKey> _eventQueue;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IMonitor<TKey> _monitor;
        protected ISignalEventQueries<TKey> _eventQueries;
        protected ISignalDispatchQueries<TKey> _dispatchQueries;
        protected SenderSettings _senderSettings;


        //init
        public BaseSignalProvider(IEventQueue<TKey> eventQueue, IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> eventSink,
            ISignalEventQueries<TKey> eventQueries, ISignalDispatchQueries<TKey> dispatchQueries, SenderSettings senderSettings)
        {
            _eventQueue = eventQueue;
            _dispatchQueue = dispatchQueue;
            _monitor = eventSink;
            _eventQueries = eventQueries;
            _dispatchQueries = dispatchQueries;
            _senderSettings = senderSettings;
        }


        //methods
        public virtual Task EnqueueMatchSubscribersEvent(Dictionary<string, string> templateDataDict, int eventKey,
            Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriptionParameters,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = eventKey,
                TopicId = topicId,
                TemplateDataDict = templateDataDict,
                SubscriberFiltersData = subscriberFiltersData
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueMatchSubscribersEvent(string templateDataObj, int eventKey,
            Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriptionParameters,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = eventKey,
                TopicId = topicId,
                TemplateDataObj = templateDataObj,
                SubscriberFiltersData = subscriberFiltersData
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateDataDict, int eventKey, 
            List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriberIds,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = eventKey,
                TopicId = topicId,
                TemplateDataDict = templateDataDict,
                PredefinedSubscriberIds = subscriberIds
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueDirectSubscriberIdsEvent(string templateDataObj, int eventKey,
            List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriberIds,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = eventKey,
                TopicId = topicId,
                TemplateDataObj = templateDataObj,
                PredefinedSubscriberIds = subscriberIds
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueDirectAddressesEvent(Dictionary<string, string> templateDataDict, int eventKey, 
            List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.DirectAddresses,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = eventKey,
                TemplateDataDict = templateDataDict,
                PredefinedAddresses = deliveryAddresses
            };
            
            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueDirectAddressesEvent(string templateDataObj, int eventKey,
            List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.DirectAddresses,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = eventKey,
                TemplateDataObj = templateDataObj,
                PredefinedAddresses = deliveryAddresses
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        protected virtual async Task EnqueueSignalEvent(SignalEvent<TKey> signalEvent, SignalWriteConcern writeConcern)
        {
            writeConcern = _senderSettings.GetWriteConcernOrDefault(writeConcern);
            bool ensurePersisted = writeConcern == SignalWriteConcern.PermanentStorage;
            if (ensurePersisted)
            {
                await _eventQueries.Insert(new List<SignalEvent<TKey>> { signalEvent })
                    .ConfigureAwait(false);
            }

            var signalWrapper = new SignalWrapper<SignalEvent<TKey>>(signalEvent, ensurePersisted);
            _eventQueue.Append(signalWrapper);
           
            _monitor.EventReceived(signalEvent);
        }

        public virtual async Task EnqueueDispatch(SignalDispatch<TKey> signalDispatch, SignalWriteConcern writeConcern)
        {
            writeConcern = _senderSettings.GetWriteConcernOrDefault(writeConcern);
            bool ensurePersisted = writeConcern == SignalWriteConcern.PermanentStorage;
            if (ensurePersisted)
            {
                await _dispatchQueries.Insert(new List<SignalDispatch<TKey>> { signalDispatch })
                    .ConfigureAwait(false);
            }

            var signalWrapper = new SignalWrapper<SignalDispatch<TKey>>(signalDispatch, ensurePersisted);
            _dispatchQueue.Append(signalWrapper);
            
            _monitor.DispatchTransferred(signalDispatch);
        }

    }
}
