﻿using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders.Interfaces;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Models;

namespace Sanatana.Notifications.SignalProviders
{
    public class DirectSignalProvider<TKey> : BaseSignalProvider<TKey>, ISignalProviderControl, ISignalProvider<TKey>
        where TKey : struct
    {
        //fields
        /// <summary>
        /// During stopping and flushing to database, need to prevent enqueue new signals
        /// </summary>
        protected bool _isStarted;


        //init
        public DirectSignalProvider(IEventQueue<TKey> eventQueues, IDispatchQueue<TKey> dispatchQueues, 
            IMonitor<TKey> eventSink, ISignalEventQueries<TKey> eventQueries, 
            ISignalDispatchQueries<TKey> dispatchQueries, SenderSettings senderSettings)
            : base(eventQueues, dispatchQueues, eventSink, eventQueries, dispatchQueries, senderSettings)
        {
            _isStarted = false;
        }


        //ISignalProviderControl
        public virtual void Start()
        {
            _isStarted = true;
        }

        public virtual void Stop(TimeSpan? timeout)
        {
            _isStarted = false;
        }


        //ISignalProvider
        protected virtual void ThrowStoppedInstanceError()
        {
            string message = string.Format(SenderInternalMessages.InMemorySignalProvider_InvokeMethodInStoppedState
                , nameof(DirectSignalProvider<TKey>), nameof(ISender));
            throw new InvalidOperationException(message);
        }

        public override Task EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int categoryId,
            Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueMatchSubscribersEvent(templateData, categoryId, subscriberFiltersData, topicId, writeConcern);
        }

        public override Task EnqueueMatchSubscribersEvent(string templateDataObj, int eventKey, Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueMatchSubscribersEvent(templateDataObj, eventKey, subscriberFiltersData, topicId, writeConcern);
        }

        public override Task EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int categoryId, 
            List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDirectSubscriberIdsEvent(templateData, categoryId, subscriberIds, topicId, writeConcern);
        }

        public override Task EnqueueDirectSubscriberIdsEvent(string templateDataObj, int eventKey, List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDirectSubscriberIdsEvent(templateDataObj, eventKey, subscriberIds, topicId, writeConcern);
        }

        public override Task EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int categoryId,
            List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDirectAddressesEvent(templateData, categoryId, deliveryAddresses, writeConcern);
        }

        public override Task EnqueueDirectAddressesEvent(string templateDataObj, int eventKey, List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDirectAddressesEvent(templateDataObj, eventKey, deliveryAddresses, writeConcern);
        }

        public override Task EnqueueDispatch(SignalDispatch<TKey> dispatch, 
            SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if(_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDispatch(dispatch, writeConcern);
        }
    }
}
