using Sanatana.Notifications.Composing.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface IEventSettingsQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<EventSettings<TKey>> items);
        Task<TotalResult<List<EventSettings<TKey>>>> Select(int page, int pageSize);
        Task<List<EventSettings<TKey>>> Select(int category);
        Task<EventSettings<TKey>> Select(TKey eventSettingsId);
        Task Update(List<EventSettings<TKey>> items);
        Task Delete(List<EventSettings<TKey>> items);
    }
}
