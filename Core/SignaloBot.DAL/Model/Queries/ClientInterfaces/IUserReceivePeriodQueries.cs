using System;
namespace SignaloBot.DAL.Queries.Client
{
    public interface IUserReceivePeriodQueries
    {
        void Delete(SignaloBot.DAL.Entities.Core.UserReceivePeriod receivePeriod, out Exception exception);
        void DeleteAll(Guid userID, out Exception exception);
        void DeleteCategory(Guid userID, int periodsDeliveryType, int periodsCategoryID, out Exception exception);
        void Insert(SignaloBot.DAL.Entities.Core.UserReceivePeriod receivePeriod, out Exception exception);
        void Rewrite(Guid userID, int periodsDeliveryType, int periodsCategoryID, System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserReceivePeriod> periods, out Exception exception);
        System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserReceivePeriod> SelectAll(Guid userID, out Exception exception);
        System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserReceivePeriod> SelectCategory(System.Collections.Generic.List<Guid> userIDs, int periodsDeliveryType, int periodsCategoryID, out Exception exception);
        System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserReceivePeriod> SelectCategory(Guid userID, int periodsDeliveryType, int periodsCategoryID, out Exception exception);
        void Update(SignaloBot.DAL.Entities.Core.UserReceivePeriod receivePeriod, out Exception exception);
    }
}
