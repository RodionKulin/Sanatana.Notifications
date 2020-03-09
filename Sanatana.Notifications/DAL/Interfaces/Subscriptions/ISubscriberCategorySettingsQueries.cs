using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberCategorySettingsQueries<TCategory, TKey>
        where TCategory : SubscriberCategorySettings<TKey>
        where TKey : struct
    {
        Task Insert(List<TCategory> items);
        Task<List<TCategory>> Select(List<TKey> subscriberIds, int categoryId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        Task<TotalResult<List<TCategory>>> Find(int pageIndex, int pageSize, bool descending);
        Task UpdateIsEnabled(List<TCategory> items);
        Task UpsertIsEnabled(List<TCategory> items);
        Task Delete(TKey subscriberId);
        Task Delete(TCategory item);
    }
}