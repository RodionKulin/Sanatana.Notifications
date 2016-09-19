using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public interface IUserCategorySettingsQueries<TKey>
        where TKey : struct
    {
        Task<bool> Delete(TKey userID);
        Task<bool> Delete(TKey userID, int deliveryType, int categoryID);
        Task<bool> Insert(List<UserCategorySettings<TKey>> settings);
        Task<QueryResult<List<UserCategorySettings<TKey>>>> Select(List<TKey> userIDs, int categoryID);
        Task<bool> UpsertIsEnabled(UserCategorySettings<TKey> settings);
    }
}