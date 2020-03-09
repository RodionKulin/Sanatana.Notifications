using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.SignalProviders.Interfaces
{
    public interface ISignalProvider<TKey>
        where TKey : struct
    {
        void EnqueueMatchSubscribersEvent(Dictionary<string, string> templateData, int categoryId, Dictionary<string, string> subscriberFiltersData = null, string topicId = null);
        void EnqueueDirectSubscriberIdsEvent(Dictionary<string, string> templateData, int categoryId, List<TKey> subscriberIds, string topicId = null);
        void EnqueueDirectAddressesEvent(Dictionary<string, string> templateData, int categoryId, List<DeliveryAddress> deliveryAddresses);
        void EnqueueDispatch(SignalDispatch<TKey> dispatch);
    }
}
