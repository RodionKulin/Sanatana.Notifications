using Sanatana.Notifications.Monitoring;
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

        public override Task EnqueueMatchSubscribersEvent(SignalDataDto signalData,
            Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueMatchSubscribersEvent(signalData, subscriberFiltersData, topicId, writeConcern);
        }

        public override Task EnqueueDirectSubscriberIdsEvent(SignalDataDto signalData,
            List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDirectSubscriberIdsEvent(signalData, subscriberIds, topicId, writeConcern);
        }

        public override Task EnqueueDirectAddressesEvent(SignalDataDto signalData,
            List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            return base.EnqueueDirectAddressesEvent(signalData, deliveryAddresses, writeConcern);
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
