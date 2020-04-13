using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface IDispatchTemplateQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<DispatchTemplate<TKey>> items);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<TotalResult<List<DispatchTemplate<TKey>>>> SelectPage(int pageIndex, int pageSize);
        Task<List<DispatchTemplate<TKey>>> SelectForEventSettings(TKey eventSettingsId);
        Task<DispatchTemplate<TKey>> Select(TKey dispatchTemplatesId);
        Task Update(List<DispatchTemplate<TKey>> items);
        Task Delete(List<DispatchTemplate<TKey>> items);
    }
}
