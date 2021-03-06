﻿using Sanatana.Notifications;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.Models;
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
        public virtual Task EnqueueMatchSubscribersEvent(SignalDataDto signalDataDto,
            Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriptionParameters,
                SubscriberFiltersData = subscriberFiltersData,
                TopicId = topicId,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = signalDataDto.EventKey,
                TemplateDataDict = signalDataDto.TemplateDataDict,
                TemplateDataObj = signalDataDto.TemplateDataObj,
                MachineName = signalDataDto.MachineName,
                ApplicationName = signalDataDto.ApplicationName
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueDirectSubscriberIdsEvent(SignalDataDto signalDataDto,
            List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.SubscriberIds,
                PredefinedSubscriberIds = subscriberIds,
                TopicId = topicId,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = signalDataDto.EventKey,
                TemplateDataDict = signalDataDto.TemplateDataDict,
                TemplateDataObj = signalDataDto.TemplateDataObj,
                MachineName = signalDataDto.MachineName,
                ApplicationName = signalDataDto.ApplicationName
            };

            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        public virtual Task EnqueueDirectAddressesEvent(SignalDataDto signalDataDto,
            List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            var signalEvent = new SignalEvent<TKey>()
            {
                AddresseeType = AddresseeType.DirectAddresses,
                PredefinedAddresses = deliveryAddresses,
                CreateDateUtc = DateTime.UtcNow,
                EventKey = signalDataDto.EventKey,
                TemplateDataDict = signalDataDto.TemplateDataDict,
                TemplateDataObj = signalDataDto.TemplateDataObj,
                MachineName = signalDataDto.MachineName,
                ApplicationName = signalDataDto.ApplicationName
            };
            
            return EnqueueSignalEvent(signalEvent, writeConcern);
        }

        protected virtual async Task EnqueueSignalEvent(SignalEvent<TKey> signalEvent, SignalWriteConcern writeConcern)
        {
            writeConcern = _senderSettings.GetWriteConcernOrDefault(writeConcern);
            bool ensurePersisted = writeConcern == SignalWriteConcern.PersistentStorage;
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
            bool ensurePersisted = writeConcern == SignalWriteConcern.PersistentStorage;
            if (ensurePersisted)
            {
                if (_senderSettings.IsDbLockStorageEnabled)
                {
                    signalDispatch.LockedBy = _senderSettings.LockedByInstanceId;
                    signalDispatch.LockedSinceUtc = DateTime.UtcNow;
                }
                await _dispatchQueries.Insert(new List<SignalDispatch<TKey>> { signalDispatch })
                    .ConfigureAwait(false);
            }

            var signalWrapper = new SignalWrapper<SignalDispatch<TKey>>(signalDispatch, ensurePersisted);
            _dispatchQueue.Append(signalWrapper);
            
            _monitor.DispatchTransferred(signalDispatch);
        }

    }
}
