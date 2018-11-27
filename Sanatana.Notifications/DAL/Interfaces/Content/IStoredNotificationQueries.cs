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
        Task<TotalResult<List<StoredNotification<TKey>>>> Select(List<TKey> subscriberIds, int page, int pageSize, bool descending);
        Task Update(List<StoredNotification<TKey>> items);
        Task Delete(List<StoredNotification<TKey>> items);
    }
}
