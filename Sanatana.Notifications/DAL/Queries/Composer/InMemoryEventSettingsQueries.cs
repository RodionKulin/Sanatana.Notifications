using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.EventsHandling;
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

        public virtual Task<List<EventSettings<TKey>>> SelectByKey(int eventKey)
        {
            List<EventSettings<TKey>> items = _items
                .Where(p => p.EventKey == eventKey)
                .ToList();

            return Task.FromResult(items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual Task<TotalResult<List<EventSettings<TKey>>>> Select(int pageIndex, int pageSize)
        {
            int skip = pageIndex * pageSize;
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
