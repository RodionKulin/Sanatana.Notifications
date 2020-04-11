using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    [ServiceContract]
    public interface ISignalService<TKey>
        where TKey : struct
    {
        [OperationContract]
        Task EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int categoryId, Dictionary<string, string> subscriberFilters = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);

        [OperationContract]
        Task EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int categoryId, List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);

        [OperationContract]
        Task EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int categoryId, List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default);

        [OperationContract]
        Task EnqueueDispatch(SignalDispatch<TKey> dispatch, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
    }
    
}
