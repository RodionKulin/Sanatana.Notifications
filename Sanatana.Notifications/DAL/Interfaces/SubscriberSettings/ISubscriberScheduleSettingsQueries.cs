using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberScheduleSettingsQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<SubscriberScheduleSettings<TKey>> periods);
        Task<List<SubscriberScheduleSettings<TKey>>> Select(List<TKey> subscriberIds, List<int> receivePeriodSets = null);
        Task RewriteSets(TKey subscriberId, List<SubscriberScheduleSettings<TKey>> periods);
        Task Delete(List<TKey> subscriberIds, List<int> receivePeriodSets = null);
    }
}