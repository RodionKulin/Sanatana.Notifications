using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberQueries<TKey>
        where TKey : struct
    {
        Task<List<Subscriber<TKey>>> Select(SubscriptionParameters parameters, SubscribersRangeParameters<TKey> subscribersRange);
        Task Update(UpdateParameters parameters, List<SignalDispatch<TKey>> items);
    }
}