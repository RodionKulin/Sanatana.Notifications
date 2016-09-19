using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public interface ISignalDispatchQueries<TKey>
        where TKey : struct
    {
        Task<bool> Insert(List<SignalDispatchBase<TKey>> items);
        Task<QueryResult<List<SignalDispatchBase<TKey>>>> SelectDelayed(TKey userID, List<KeyValuePair<int, int>> deliveryTypeAndCategories);
        Task<bool> UpdateCounters(UpdateParameters parameters, List<SignalDispatchBase<TKey>> items);
        Task<bool> UpdateSendDateUtc(List<SignalDispatchBase<TKey>> items);
    }
}