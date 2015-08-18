using System;
namespace SignaloBot.DAL.Queries.Client
{
    public interface IUserCategorySettingsQueries
    {
        void DeleteAll(Guid userID, out Exception exception);
        void Delete(Guid userID, int deliveryType, int categoryID, out Exception exception);
        void Insert(SignaloBot.DAL.Entities.Core.UserCategorySettings settings, out Exception exception);
        void Insert(System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserCategorySettings> settings, out Exception exception);
        System.Collections.Generic.List<SignaloBot.DAL.Entities.Core.UserCategorySettings> Select(System.Collections.Generic.List<Guid> userIDs, int categoryID, out Exception exception);
        void UpsertIsEnabled(SignaloBot.DAL.Entities.Core.UserCategorySettings settings, out Exception exception);
    }
}
