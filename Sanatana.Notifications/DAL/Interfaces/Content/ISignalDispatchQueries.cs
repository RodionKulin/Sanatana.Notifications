using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalDispatchQueries<TKey> : ISignalQueries<SignalDispatch<TKey>>
        where TKey : struct
    {
        Task<List<SignalDispatch<TKey>>> SelectScheduled(TKey subscriberId, List<(int deliveryType, int category)> categories);
        Task<List<SignalDispatch<TKey>>> Select(int count, List<int> deliveryTypes, int maxFailedAttempts);
    }
}