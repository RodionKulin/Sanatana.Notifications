using Sanatana.Notifications.EventTracking;
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
    public class DirectSignalProvider<TKey> : BaseSignalProvider<TKey>, ISignalProviderControl, ISignalProvider<TKey>
        where TKey : struct
    {
        //fields
        protected bool _isStarted;


        //init
        public DirectSignalProvider(IEventQueue<TKey> eventQueues, IDispatchQueue<TKey> dispatchQueues, IEventTracker<TKey> eventSink)
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


        //templateData transfer
        protected virtual void ThrowStoppedInstanceError()
        {
            string message = string.Format(SenderInternalMessages.InMemorySignalProvider_InvokeMethodInStoppedState
                , nameof(DirectSignalProvider<TKey>), nameof(ISender));
            throw new InvalidOperationException(message);
        }

        public override void EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int categoryId,
            Dictionary<string, string> subscriberFiltersData = null, string topicId = null)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.EnqueueMatchSubscribersEvent(templateData, categoryId, subscriberFiltersData, topicId);
        }

        public override void EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int categoryId, 
            List<TKey> subscriberIds, string topicId = null)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.EnqueueDirectSubscriberIdsEvent(templateData, categoryId, subscriberIds, topicId);
        }

        public override void EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int categoryId,
            List<DeliveryAddress> deliveryAddresses)
        {
            if (_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.EnqueueDirectAddressesEvent(templateData, categoryId, deliveryAddresses);
        }

        public override void EnqueueDispatch(SignalDispatch<TKey> dispatch)
        {
            if(_isStarted == false)
            {
                ThrowStoppedInstanceError();
            }

            base.EnqueueDispatch(dispatch);
        }
    }
}
