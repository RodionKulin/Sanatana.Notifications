using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders.Interfaces
{
    public interface ISignalProvider<TKey>
        where TKey : struct
    {
        Task EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int eventKey, Dictionary<string, string> subscriberFiltersData = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
        Task EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int eventKey, List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
        Task EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int eventKey, List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
        Task EnqueueDispatch(SignalDispatch<TKey> dispatch, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
    }
}
