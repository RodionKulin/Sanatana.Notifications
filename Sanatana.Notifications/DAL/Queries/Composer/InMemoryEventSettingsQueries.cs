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
    public class InMemoryEventSettingsQueries<TKey> : IEventSettingsQueries<TKey>
        where TKey : struct
    {
        //fields
        protected List<EventSettings<TKey>> _items;


        //init
        public InMemoryEventSettingsQueries(IEnumerable<EventSettings<TKey>> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            _items = items.ToList();
        }


        //methods
        public virtual Task Insert(List<EventSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task<EventSettings<TKey>> Select(TKey eventSettingsId)
        {
            EventSettings<TKey> item = _items.FirstOrDefault(
                p => EqualityComparer<TKey>.Default.Equals(p.EventSettingsId, eventSettingsId));
                        
            return Task.FromResult(item);
        }

        public virtual Task<List<EventSettings<TKey>>> Select(int category)
        {
            List<EventSettings<TKey>> items = _items
                .Where(p => p.CategoryId == category)
                .ToList();

            return Task.FromResult(items);
        }

        public virtual Task<TotalResult<List<EventSettings<TKey>>>> Select(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            List<EventSettings<TKey>> list = _items.Skip(skip).Take(pageSize).ToList();
            var result = new TotalResult<List<EventSettings<TKey>>>(list, _items.Count);
            return Task.FromResult(result);
        }

        public virtual Task Update(List<EventSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

        public virtual Task Delete(List<EventSettings<TKey>> items)
        {
            throw new NotImplementedException();
        }

    }
}
