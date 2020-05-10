using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders.Interfaces
{
    public interface ISignalProvider<TKey>
        where TKey : struct
    {
        Task EnqueueMatchSubscribersEvent(SignalDataDto signalDataDto, Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
        Task EnqueueDirectSubscriberIdsEvent(SignalDataDto signalDataDto, List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
        Task EnqueueDirectAddressesEvent(SignalDataDto signalDataDto, List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
        Task EnqueueDispatch(SignalDispatch<TKey> dispatch, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
    }
}
