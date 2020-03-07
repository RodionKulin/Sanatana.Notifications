using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface IStoredNotificationQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<StoredNotification<TKey>> items);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberIds"></param>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        Task<TotalResult<List<StoredNotification<TKey>>>> Select(List<TKey> subscriberIds, int pageIndex, int pageSize, bool descending);
        Task Update(List<StoredNotification<TKey>> items);
        Task Delete(List<StoredNotification<TKey>> items);
    }
}
