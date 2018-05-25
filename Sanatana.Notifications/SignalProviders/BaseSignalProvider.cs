using Sanatana.Notifications;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Monitoring;
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
        protected IMonitor<TKey> _eventSink;

        
        //init
        public BaseSignalProvider(IEventQueue<TKey> eventQueue
            , IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> eventSink)
        {
            _eventQueue = eventQueue;
            _dispatchQueue = dispatchQueue;
            _eventSink = eventSink;
        }
        

        //methods
        public virtual void RaiseEventAndMatchSubscribers(Dictionary<string, string> data, int categoryId, string topicId = null, TKey? subscribersGroupId = null)
        {
            var keyValueEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.AllSubscsribers,
                CreateDateUtc = DateTime.UtcNow,
                CategoryId = categoryId,
                TopicId = topicId,
                DataKeyValues = data,
                GroupId = subscribersGroupId
            };

            EnqueueSignalEvent(keyValueEvent);
        }

        public virtual void RaiseEventForSubscribersDirectly(List<TKey> subscriberIds, Dictionary<string, string> data, int categoryId, string topicId = null)
        {
            var keyValueEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SpecifiedSubscribersById,
                CreateDateUtc = DateTime.UtcNow,
                CategoryId = categoryId,
                TopicId = topicId,
                DataKeyValues = data,
                PredefinedSubscriberIds = subscriberIds
            };

            EnqueueSignalEvent(keyValueEvent);
        }

        public virtual void RaiseEventForAddressesDirectly(List<DeliveryAddress> deliveryAddresses, Dictionary<string, string> data, int categoryId)
        {
            var keyValueEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.DirectAddresses,
                CreateDateUtc = DateTime.UtcNow,
                CategoryId = categoryId,
                DataKeyValues = data,
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

        public virtual void SendDispatch(SignalDispatch<TKey> dispatch)
        {
            var signalWrapper = new SignalWrapper<SignalDispatch<TKey>>(dispatch, false);

            _dispatchQueue.Append(signalWrapper);
            
            _eventSink.DispatchTransferred(dispatch);
        }

    }
}
