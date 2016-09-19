using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public class LocalComposerQueries<TKey> : IComposerQueries<TKey>
        where TKey : struct
    {
        //поля
        protected List<ComposerSettings<TKey>> _items;



        //инициализация
        public LocalComposerQueries(List<ComposerSettings<TKey>> items)
        {
            _items = items;

            if(_items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
        }


        //методы

        public virtual Task<bool> Insert(List<ComposerSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task<QueryResult<ComposerSettings<TKey>>> Select(TKey composerSettingsID)
        {
            ComposerSettings<TKey> item = _items
                .FirstOrDefault(p => EqualityComparer<TKey>.Default.Equals(p.ComposerSettingsID, composerSettingsID));
                        
            return Task.FromResult(new QueryResult<ComposerSettings<TKey>>(item, false));
        }

        public virtual Task<QueryResult<List<ComposerSettings<TKey>>>> Select(int category)
        {
            List<ComposerSettings<TKey>> items = _items
                .Where(p => p.CategoryID == category).ToList();

            return Task.FromResult(new QueryResult<List<ComposerSettings<TKey>>>(items, false));
        }

        public virtual Task<bool> Update(List<ComposerSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Delete(List<ComposerSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }
    }
}
