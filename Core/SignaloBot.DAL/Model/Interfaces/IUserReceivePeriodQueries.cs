using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public interface IUserReceivePeriodQueries<TKey>
        where TKey : struct
    {
        Task<bool> Delete(TKey userID);
        Task<bool> Delete(TKey userID, List<int> receivePeriodsGroups);
        Task<bool> Insert(List<UserReceivePeriod<TKey>> periods);
        Task<bool> Rewrite(TKey userID, int receivePeriodsGroup, List<UserReceivePeriod<TKey>> periods);
        Task<QueryResult<List<UserReceivePeriod<TKey>>>> Select(TKey userID);      
        Task<QueryResult<List<UserReceivePeriod<TKey>>>> Select(List<TKey> userIDs, List<int> receivePeriodsGroups);
    }
}