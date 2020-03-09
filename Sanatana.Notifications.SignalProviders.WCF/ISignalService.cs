using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
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
        void EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int categoryId, Dictionary<string, string> subscriberFilters = null, string topicId = null);

        [OperationContract]
        void EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int categoryId, List<TKey> subscriberIds, string topicId = null);

        [OperationContract]
        void EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int categoryId, List<DeliveryAddress> deliveryAddresses);

        [OperationContract]
        void EnqueueDispatch(SignalDispatch<TKey> dispatch);
    }
    
}
