using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberCategorySettingsQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<SubscriberCategorySettings<TKey>> items);
        Task<List<SubscriberCategorySettings<TKey>>> Select(List<TKey> subscriberIds, int categoryId);
        Task<TotalResult<List<SubscriberCategorySettings<TKey>>>> Select(int page, int pageSize, bool descending);
        Task UpdateIsEnabled(List<SubscriberCategorySettings<TKey>> items);
        Task UpsertIsEnabled(List<SubscriberCategorySettings<TKey>> items);
        Task Delete(TKey subscriberId);
        Task Delete(SubscriberCategorySettings<TKey> item);
    }
}