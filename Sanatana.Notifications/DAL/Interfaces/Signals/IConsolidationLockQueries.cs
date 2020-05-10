using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface IConsolidationLockQueries<TKey> 
        where TKey : struct
    {

        Task<bool> InsertOneHandleDuplicate(ConsolidationLock<TKey> entity, CancellationToken token = default);

        Task<ConsolidationLock<TKey>> FindExistingMatch(ConsolidationLock<TKey> consolidationLock, CancellationToken token = default);

        Task<List<ConsolidationLock<TKey>>> FindAll(Expression<Func<ConsolidationLock<TKey>, bool>> filterConditions = null, CancellationToken token = default);

        Task<bool> ExtendLockTime(ConsolidationLock<TKey> lockToExtend, CancellationToken token = default);
        
        Task Delete(ConsolidationLock<TKey>[] locksToRemove, CancellationToken token = default);
    }
}
