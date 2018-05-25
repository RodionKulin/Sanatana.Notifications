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
        void RaiseEventAndMatchSubscribers(Dictionary<string, string> data, int categoryId, string topicId = null, TKey? subscribersGroupId = null);

        [OperationContract]
        void RaiseEventForSubscribersDirectly(List<TKey> subscriberIds, Dictionary<string, string> data, int categoryId, string topicId = null);

        [OperationContract]
        void RaiseEventForAddressesDirectly(List<DeliveryAddress> deliveryAddresses, Dictionary<string, string> data, int categoryId);

        [OperationContract]
        void SendDispatch(SignalDispatch<TKey> dispatch);
    }
    
}
