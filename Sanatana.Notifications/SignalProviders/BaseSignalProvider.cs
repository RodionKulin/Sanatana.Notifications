using Sanatana.Notifications;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.EventTracking;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.SignalProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.SignalProviders
{
    public abstract class BaseSignalProvider<TKey>
        where TKey : struct
    {
        //fields
        protected IEventQueue<TKey> _eventQueue;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IEventTracker<TKey> _eventSink;

        
        //init
        public BaseSignalProvider(IEventQueue<TKey> eventQueue, IDispatchQueue<TKey> dispatchQueue, IEventTracker<TKey> eventSink)
        {
            _eventQueue = eventQueue;
            _dispatchQueue = dispatchQueue;
            _eventSink = eventSink;
        }


        //methods
        public virtual void EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int categoryId,
            Dictionary<string, string> subscriberFilters = null, string topicId = null)
        {
            var keyValueEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriptionParameters,
                CreateDateUtc = DateTime.UtcNow,
                CategoryId = categoryId,
                TopicId = topicId,
                TemplateData = templateData,
                SubscriberFiltersData = subscriberFilters
            };

            EnqueueSignalEvent(keyValueEvent);
        }

        public virtual void EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int categoryId, 
            List<TKey> subscriberIds, string topicId = null)
        {
            var keyValueEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriberIds,
                CreateDateUtc = DateTime.UtcNow,
                CategoryId = categoryId,
                TopicId = topicId,
                TemplateData = templateData,
                PredefinedSubscriberIds = subscriberIds
            };

            EnqueueSignalEvent(keyValueEvent);
        }

        public virtual void EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int categoryId, 
            List<DeliveryAddress> deliveryAddresses)
        {
            var keyValueEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.DirectAddresses,
                CreateDateUtc = DateTime.UtcNow,
                CategoryId = categoryId,
                TemplateData = templateData,
                PredefinedAddresses = deliveryAddresses
            };
            
            EnqueueSignalEvent(keyValueEvent);
        }

        protected virtual void EnqueueSignalEvent(SignalEvent<TKey> keyValueEvent)
        {
            var signalWrapper = new SignalWrapper<SignalEvent<TKey>>(keyValueEvent, false);

            _eventQueue.Append(signalWrapper);
           
            _eventSink.EventTransferred(keyValueEvent);
        }

        public virtual void EnqueueDispatch(SignalDispatch<TKey> dispatch)
        {
            var signalWrapper = new SignalWrapper<SignalDispatch<TKey>>(dispatch, false);

            _dispatchQueue.Append(signalWrapper);
            
            _eventSink.DispatchTransferred(dispatch);
        }

    }
}
