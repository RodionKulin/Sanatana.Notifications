using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public interface IUserTopicSettingsQueries<TKey>
        where TKey : struct
    {
        Task<bool> Insert(List<UserTopicSettings<TKey>> settings);
        Task<QueryResult<UserTopicSettings<TKey>>> Select(TKey userID, int categoryID, string topicID);
        Task<TotalResult<List<UserTopicSettings<TKey>>>> SelectPage(TKey userID, List<int> categoryIDs, int page, int count);
        Task<bool> Delete(TKey userID);
        Task<bool> Delete(TKey userID, int categoryID, string topicID);        
        Task<bool> UpdateIsDeleted(UserTopicSettings<TKey> settings);
        Task<bool> Upsert(UserTopicSettings<TKey> settings, bool updateExisting);
    }
}