using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public interface IUserDeliveryTypeSettingsQueries<TKey>
        where TKey : struct
    {
        Task<QueryResult<bool>> CheckAddressExists(int deliveryType, string address);
        Task<bool> Delete(TKey userID);
        Task<bool> Delete(TKey userID, int deliveryType);
        Task<bool> DisableAllDeliveryTypes(TKey userID);
        Task<bool> Insert(List<UserDeliveryTypeSettings<TKey>> settings);
        Task<bool> ResetNDRCount(TKey userID, int deliveryType);
        Task<QueryResult<List<UserDeliveryTypeSettings<TKey>>>> Select(TKey userID);
        Task<QueryResult<List<UserDeliveryTypeSettings<TKey>>>> Select(int deliveryType, List<string> addresses);
        Task<QueryResult<UserDeliveryTypeSettings<TKey>>> Select(TKey userID, int deliveryType);
        Task<TotalResult<List<UserDeliveryTypeSettings<TKey>>>> SelectPage(List<int> deliveryTypes, int page, int pageSize);
        Task<bool> Update(UserDeliveryTypeSettings<TKey> settings);
        Task<bool> UpdateAddress(TKey userID, int deliveryType, string address);
        Task<bool> UpdateLastVisit(TKey userID);
        Task<bool> UpdateNDRResetCode(TKey userID, int deliveryType, string resetCode);
        Task<bool> UpdateNDRSettings(List<UserDeliveryTypeSettings<TKey>> settings);
        Task<bool> UpdateTimeZone(TKey userID, TimeZoneInfo timeZone);
    }
}