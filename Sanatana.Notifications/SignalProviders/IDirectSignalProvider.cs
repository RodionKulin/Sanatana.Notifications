using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.SignalProviders
{
    public interface IDirectSignalProvider<TKey>
        where TKey : struct
    {
        void RaiseEventAndMatchSubscribers(Dictionary<string, string> data, int categoryId, string topicId = null, TKey? subscribersGroupId = null);
        void RaiseEventForSubscribersDirectly(List<TKey> subscriberIds, Dictionary<string, string> data, int categoryId, string topicId = null);
        void RaiseEventForAddressesDirectly(List<DeliveryAddress> deliveryAddresses, Dictionary<string, string> data, int categoryId);
        void SendDispatch(SignalDispatch<TKey> dispatch);
    }
}
