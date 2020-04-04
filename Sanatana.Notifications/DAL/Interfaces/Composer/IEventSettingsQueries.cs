using Sanatana.Notifications.EventsHandling.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface IEventSettingsQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<EventSettings<TKey>> items);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<TotalResult<List<EventSettings<TKey>>>> Select(int pageIndex, int pageSize);
        Task<List<EventSettings<TKey>>> SelectByKey(int eventKey);
        Task<EventSettings<TKey>> Select(TKey eventSettingsId);
        Task Update(List<EventSettings<TKey>> items);
        Task Delete(List<EventSettings<TKey>> items);
    }
}
