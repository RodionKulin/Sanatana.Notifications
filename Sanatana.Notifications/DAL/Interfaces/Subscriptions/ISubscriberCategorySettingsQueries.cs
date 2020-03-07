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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        Task<TotalResult<List<SubscriberCategorySettings<TKey>>>> Find(int pageIndex, int pageSize, bool descending);
        Task UpdateIsEnabled(List<SubscriberCategorySettings<TKey>> items);
        Task UpsertIsEnabled(List<SubscriberCategorySettings<TKey>> items);
        Task Delete(TKey subscriberId);
        Task Delete(SubscriberCategorySettings<TKey> item);
    }
}