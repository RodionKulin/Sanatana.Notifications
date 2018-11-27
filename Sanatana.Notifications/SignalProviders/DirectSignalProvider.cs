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

namespace Sanatana.Notifications.SignalProviders
{
    public class DirectSignalProvider<TKey> : BaseSignalProvider<TKey>, ISignalProviderControl, IDirectSignalProvider<TKey>
        where TKey : struct
    {
        //fields
        protected bool _isStarted;


        //init
        public DirectSignalProvider(IEventQueue<TKey> eventQueues
            , IDispatchQueue<TKey> dispatchQueues, IMonitor<TKey> eventSink)
            : base(eventQueues, dispatchQueues, eventSink)
        {
            _isStarted = false;
        }


        //control
        public virtual void Start()
        {
            _isStarted = true;
        }

        public virtual void Stop(TimeSpan? timeout)
        {
            _isStarted = false;
        }


        //data transfer
        protected virtual void ThrowStoppedInstanceError()
        {
            string message = string.Format(SenderInternalMessages.InMemorySignalProvider_InvokeMethodInStoppedState
                , nameof(DirectSignalProvider<TKey>), nameof(ISender));
            throw new InvalidOperationException(message);
        }

        public override void RaiseEventAndMatchSubscribers(Dictionary<string, string> data, int categoryId, string topicId = null, TKey? subscribersGroupId = null)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.RaiseEventAndMatchSubscribers(data, categoryId, topicId, subscribersGroupId);
        }

        public override void RaiseEventForSubscribersDirectly(List<TKey> subscriberIds, Dictionary<string, string> data, int categoryId, string topicId = null)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.RaiseEventForSubscribersDirectly(subscriberIds, data, categoryId, topicId);
        }

        public override void RaiseEventForAddressesDirectly(List<DeliveryAddress> deliveryAddresses, Dictionary<string, string> data, int categoryId)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.RaiseEventForAddressesDirectly(deliveryAddresses, data, categoryId);
        }

        public override void SendDispatch(SignalDispatch<TKey> dispatch)
        {
            if(_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.SendDispatch(dispatch);
        }
    }
}
