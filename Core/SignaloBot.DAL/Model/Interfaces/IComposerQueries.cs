using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public interface IComposerQueries<TKey>
        where TKey : struct
    {
        Task<bool> Insert(List<ComposerSettings<TKey>> items);

        Task<QueryResult<List<ComposerSettings<TKey>>>> Select(int category);

        Task<QueryResult<ComposerSettings<TKey>>> Select(TKey composerSettingsID);

        Task<bool> Update(List<ComposerSettings<TKey>> items);

        Task<bool> Delete(List<ComposerSettings<TKey>> items);
    }
}
