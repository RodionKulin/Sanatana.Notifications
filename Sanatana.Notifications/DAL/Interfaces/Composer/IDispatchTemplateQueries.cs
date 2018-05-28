using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.Composing.Templates;
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
        Task<TotalResult<List<DispatchTemplate<TKey>>>> Select(int page, int pageSize);
        Task<List<DispatchTemplate<TKey>>> SelectForEventSettings(TKey eventSettingsId);
        Task<DispatchTemplate<TKey>> Select(TKey dispatchTemplatesId);
        Task Update(List<DispatchTemplate<TKey>> items);
        Task Delete(List<DispatchTemplate<TKey>> items);
    }
}
