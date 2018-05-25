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
    public interface IComposerSettingsQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<ComposerSettings<TKey>> items);
        Task<TotalResult<List<ComposerSettings<TKey>>>> Select(int page, int pageSize);
        Task<List<ComposerSettings<TKey>>> Select(int category);
        Task<ComposerSettings<TKey>> Select(TKey composerSettingsId);
        Task Update(List<ComposerSettings<TKey>> items);
        Task Delete(List<ComposerSettings<TKey>> items);
    }
}
