using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalDispatchQueries<TKey> : ISignalQueries<SignalDispatch<TKey>>
        where TKey : struct
    {
        Task<List<SignalDispatch<TKey>>> SelectConsolidated(int pageSize, List<TKey> subscriberIds, List<(int deliveryType, int category)> categories, DateTime createdBefore, DateTime? createdAfter = null);
        Task<List<SignalDispatch<TKey>>> Select(int count, List<int> deliveryTypes, int maxFailedAttempts, TKey[] excludeIds);
        Task<List<SignalDispatch<TKey>>> SelectWithSetLock(int count, List<int> deliveryTypes,
            int maxFailedAttempts, TKey[] excludeIds, Guid lockId, DateTime lockExpirationDate);
        Task<List<SignalDispatch<TKey>>> SelectLocked(int count, List<int> deliveryTypes,
            int maxFailedAttempts, TKey[] excludeIds, Guid lockId, DateTime lockExpirationDate);
        Task<bool> SetLock(List<TKey> dispatchIds, Guid lockId, DateTime lockStartDate, DateTime lockExpirationDate);
        Task DeleteConsolidated(List<SignalDispatch<TKey>> item);
    }
}