using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISubscriberTopicSettingsQueries<TTopic, TKey>
        where TTopic : SubscriberTopicSettings<TKey>
        where TKey : struct
    {
        Task Insert(List<TTopic> settings);
        Task<TTopic> Select(TKey subscriberId, int categoryId, string topicId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="subscriberIds"></param>
        /// <param name="deliveryTypeIds"></param>
        /// <param name="categoryIds"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        Task<TotalResult<List<TTopic>>> SelectPage(int pageIndex, int pageSize, List<TKey> subscriberIds = null,
            List<int> deliveryTypeIds = null, List<int> categoryIds = null, List<string> topics = null);
        Task UpdateIsEnabled(List<TTopic> items);
        Task UpsertIsEnabled(List<TTopic> items);
        Task UpdateIsDeleted(TTopic settings);
        Task Upsert(TTopic settings, bool updateExisting);
        Task Delete(TKey subscriberId);
        Task Delete(List<TKey> subscriberIds = null, List<int> deliveryTypeIds = null
            , List<int> categoryIds = null, List<string> topicIds = null);
    }
}