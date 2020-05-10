using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Models;
using Sanatana.Notifications.SignalProviders.WCF.Dtos;
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
        Task MatchSubscribersEvent(SignalDataDC signalDataDto, Dictionary<string, string> subscriberFilters = null, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);

        [OperationContract]
        Task DirectSubscriberIdsEvent(SignalDataDC signalDataDto, List<TKey> subscriberIds, string topicId = null, SignalWriteConcern writeConcern = SignalWriteConcern.Default);

        [OperationContract]
        Task DirectAddressesEvent(SignalDataDC signalDataDto, List<DeliveryAddress> deliveryAddresses, SignalWriteConcern writeConcern = SignalWriteConcern.Default);

        [OperationContract]
        Task Dispatch(SignalDispatch<TKey> dispatch, SignalWriteConcern writeConcern = SignalWriteConcern.Default);
    }
    
}
