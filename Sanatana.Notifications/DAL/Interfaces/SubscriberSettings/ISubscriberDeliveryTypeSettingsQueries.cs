using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberDeliveryTypeSettingsQueries<TKey>
        where TKey : struct
    {
        Task<bool> CheckAddressExists(int deliveryType, string address);
        Task Delete(TKey subscriberId);
        Task Delete(TKey subscriberId, int deliveryType);
        Task DisableAllDeliveryTypes(TKey subscriberId);
        Task Insert(List<SubscriberDeliveryTypeSettings<TKey>> settings);
        Task ResetNDRCount(TKey subscriberId, int deliveryType);
        Task<List<SubscriberDeliveryTypeSettings<TKey>>> Select(TKey subscriberId);
        Task<List<SubscriberDeliveryTypeSettings<TKey>>> Select(int deliveryType, List<string> addresses);
        Task<SubscriberDeliveryTypeSettings<TKey>> Select(TKey subscriberId, int deliveryType);
        Task<TotalResult<List<SubscriberDeliveryTypeSettings<TKey>>>> SelectPage(List<int> deliveryTypes, int page, int pageSize);
        Task Update(SubscriberDeliveryTypeSettings<TKey> settings);
        Task UpdateAddress(TKey subscriberId, int deliveryType, string address);
        Task UpdateLastVisit(TKey subscriberId);
        Task UpdateNDRResetCode(TKey subscriberId, int deliveryType, string resetCode);
        Task UpdateNDRSettings(List<SubscriberDeliveryTypeSettings<TKey>> settings);
        Task UpdateTimeZone(TKey subscriberId, TimeZoneInfo timeZone);
    }
}