using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Queries
{
    public class InMemoryComposerSettingsQueries<TKey> : IComposerSettingsQueries<TKey>
        where TKey : struct
    {
        //fields
        protected List<ComposerSettings<TKey>> _items;


        //init
        public InMemoryComposerSettingsQueries(IEnumerable<ComposerSettings<TKey>> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            _items = items.ToList();
        }


        //methods
        public virtual Task Insert(List<ComposerSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task<ComposerSettings<TKey>> Select(TKey composerSettingsId)
        {
            ComposerSettings<TKey> item = _items.FirstOrDefault(
                p => EqualityComparer<TKey>.Default.Equals(p.ComposerSettingsId, composerSettingsId));
                        
            return Task.FromResult(item);
        }

        public virtual Task<List<ComposerSettings<TKey>>> Select(int category)
        {
            List<ComposerSettings<TKey>> items = _items
                .Where(p => p.CategoryId == category)
                .ToList();

            return Task.FromResult(items);
        }

        public virtual Task<TotalResult<List<ComposerSettings<TKey>>>> Select(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            List<ComposerSettings<TKey>> list = _items.Skip(skip).Take(pageSize).ToList();
            var result = new TotalResult<List<ComposerSettings<TKey>>>(list, _items.Count);
            return Task.FromResult(result);
        }

        public virtual Task Update(List<ComposerSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task Delete(List<ComposerSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

    }
}
