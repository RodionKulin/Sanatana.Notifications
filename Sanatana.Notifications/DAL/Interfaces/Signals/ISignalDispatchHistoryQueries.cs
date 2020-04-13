using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalDispatchHistoryQueries<TKey>
        where TKey : struct
    {
        Task InsertMany(IEnumerable<SignalDispatch<TKey>> entities, CancellationToken token = default);

        Task<List<SignalDispatch<TKey>>> FindMany(Expression<Func<SignalDispatch<TKey>, bool>> filterConditions, 
            int pageIndex, int pageSize, bool orderDescending = false, 
            Expression<Func<SignalDispatch<TKey>, object>> orderExpression = null, CancellationToken token = default);

        Task<long> DeleteMany(Expression<Func<SignalDispatch<TKey>, bool>> filterConditions, CancellationToken token = default);

        Task<long> CountDocuments(Expression<Func<SignalDispatch<TKey>, bool>> filterConditions, CancellationToken token = default);

        Task<long> ReplaceMany(IEnumerable<SignalDispatch<TKey>> entities, bool isUpsert, CancellationToken token = default);

    }
}
