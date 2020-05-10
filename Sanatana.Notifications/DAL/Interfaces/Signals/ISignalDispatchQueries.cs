using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalDispatchQueries<TKey> : ISignalQueries<SignalDispatch<TKey>>
        where TKey : struct
    {
        Task<List<SignalDispatch<TKey>>> SelectConsolidated(int pageSize, List<TKey> subscriberIds, List<(int deliveryType, int category)> categories, DateTime createdBefore, DateTime? createdAfter = null);
        Task<List<SignalDispatch<TKey>>> SelectNotSetLock(DispatchQueryParameters<TKey> parameters);
        Task<List<SignalDispatch<TKey>>> SelectWithSetLock(DispatchQueryParameters<TKey> parameters, Guid lockId, DateTime lockExpirationDate);
        Task<List<SignalDispatch<TKey>>> SelectLocked(DispatchQueryParameters<TKey> parameters, Guid lockId, DateTime lockExpirationDate);
        Task<bool> SetLock(List<TKey> dispatchIds, Guid lockId, DateTime newLockSinceTimeUtc, DateTime existingLockSinceDateUtc);
        Task DeleteConsolidated(List<SignalDispatch<TKey>> item);
    }
}