using SignaloBot.DAL.Entities.Core;
using System;
using System.Collections.Generic;

namespace SignaloBot.DAL.Queries.Client
{
    public interface IUserDeliveryTypeSettingsQueries
    {
        void DeleteAll(Guid userID, out Exception exception);
        void Delete(Guid userID, int deliveryType, out Exception exception);
        void DisableAllDeliveryTypes(Guid userID, out Exception exception);
        void Insert(SignaloBot.DAL.Entities.Core.UserDeliveryTypeSettings userDeliveryTypeSettings, out Exception exception);
        void ResetNDRCount(Guid userID, int deliveryType, out Exception exception);
        List<UserDeliveryTypeSettings> SelectAllUserDeliveryTypes(Guid userID, out Exception exception);
        UserDeliveryTypeSettings Find(Guid userID, int deliveryType, out Exception exception);
        void UpdateNDRResetCode(Guid userID, int deliveryType, string resetCode, out Exception exception);
        void Update(SignaloBot.DAL.Entities.Core.UserDeliveryTypeSettings userDeliveryTypeSettings, out Exception exception);
        void UpdateLastVisit(Guid userID, out Exception exception);
        void UpdateTimeZone(Guid userID, TimeZoneInfo timeZone, out Exception exception);
        bool CheckAddressExists(string address, int deliveryType, out Exception exception);
        List<UserDeliveryTypeSettings> SelectAllDeliveryTypes(int start, int end, out int total, out Exception exception);
        void UpdateAddress(Guid userID, int deliveryType, string address, out Exception exception);
    }
}
