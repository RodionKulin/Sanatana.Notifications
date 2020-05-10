using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.SignalProviders.WCF.Dtos;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [ServiceKnownType(nameof(SenderSettings.GetKnownServiceTypes), typeof(SenderSettings))]
    public class SignalService<TKey> : BaseSignalProvider<TKey>, ISignalService<TKey>
        where TKey : struct
    {
        public SignalService(IEventQueue<TKey> eventQueue, IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> eventSink,
            ISignalEventQueries<TKey> eventQueries, ISignalDispatchQueries<TKey> dispatchQueries, SenderSettings senderSettings)
            : base(eventQueue, dispatchQueue, eventSink, eventQueries, dispatchQueries, senderSettings)
        {
        }

        //methods
        [OperationContract]
        public Task MatchSubscribersEvent(SignalDataDC signalDataDto, Dictionary<string, string> subscriberFilters = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            return base.EnqueueMatchSubscribersEvent(signalDataDto, subscriberFilters, topicId, writeConcern);
        }

        [OperationContract]
        public Task DirectSubscriberIdsEvent(SignalDataDC signalDataDto, List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            return base.EnqueueDirectSubscriberIdsEvent(signalDataDto, subscriberIds, topicId, writeConcern);
        }

        [OperationContract]
        public Task DirectAddressesEvent(SignalDataDC signalDataDto, List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {

            return base.EnqueueDirectAddressesEvent(signalDataDto, deliveryAddresses, writeConcern);
        }

        [OperationContract]
        public Task Dispatch(SignalDispatch<TKey> dispatch, SignalWriteConcern writeConcern = SignalWriteConcern.Default)
        {
            return base.EnqueueDispatch(dispatch, writeConcern);
        }
    }
}
