using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalBounceQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<SignalBounce<TKey>> items);
        Task<TotalResult<List<SignalBounce<TKey>>>> Select(
             int page, int pageSize, List<TKey> receiverSubscriberIds = null);
        Task Delete(List<TKey> receiverSubscriberIds);
        Task Delete(List<SignalBounce<TKey>> items);
    }
}