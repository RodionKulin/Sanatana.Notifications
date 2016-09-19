using Common.Utility;
using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public interface ISignalDispatchQueueQueries<TKey> : IDisposable
        where TKey : struct
    {
        Task<bool> Insert(List<SignalDispatchBase<TKey>> items);

        Task<QueryResult<List<SignalDispatchBase<TKey>>>> Select(
            int count, List<int> deliveryTypes, int maxFailedAttempts);

        Task<bool> UpdateFailedAttempts(List<SignalDispatchBase<TKey>> items);

        Task<bool> Delete(List<SignalDispatchBase<TKey>> items);
    }
}
