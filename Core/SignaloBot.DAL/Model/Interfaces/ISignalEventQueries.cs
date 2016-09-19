using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public interface ISignalEventQueries<TKey> : IDisposable
        where TKey : struct
    {
        Task<bool> Insert(List<SignalEventBase<TKey>> items);

        Task<QueryResult<List<SignalEventBase<TKey>>>> Select(int count, int maxFailedAttempts);

        Task<bool> Update(List<SignalEventBase<TKey>> items);

        Task<bool> Delete(List<SignalEventBase<TKey>> items);
    }
}
