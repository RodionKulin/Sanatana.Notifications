using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Queries
{
    public class CachedEventSettingsQueries<TKey> : IEventSettingsQueries<TKey>
        where TKey : struct
    {
        //fields
        protected IEventSettingsQueries<TKey> _storageQueries;
        protected TotalResult<List<EventSettings<TKey>>> _cache;
        protected IChangeNotifier<EventSettings<TKey>> _changeNotifier;


        //init
        public CachedEventSettingsQueries(IEventSettingsQueries<TKey> storageQueries)
        {
            _storageQueries = storageQueries;
        }

        public CachedEventSettingsQueries(IEventSettingsQueries<TKey> storageQueries
            , IChangeNotifier<EventSettings<TKey>> changeNotifier)
        {
            _storageQueries = storageQueries;
            _changeNotifier = changeNotifier;
        }


        //methods
        public virtual async Task Insert(List<EventSettings<TKey>> items)
        {
            await _storageQueries.Insert(items).ConfigureAwait(false);
            _cache = null;
        }

        public virtual async Task<EventSettings<TKey>> Select(TKey eventSettingsId)
        {
            TotalResult<List<EventSettings<TKey>>> allItems = await GetFromCacheOrFetch()
                .ConfigureAwait(false);

            EventSettings<TKey> item = allItems.Data.FirstOrDefault(
                x => EqualityComparer<TKey>.Default.Equals(x.EventSettingsId, eventSettingsId));
            return item;
        }

        public virtual async Task<List<EventSettings<TKey>>> SelectByKey(int eventKey)
        {
            TotalResult<List<EventSettings<TKey>>> allItems = await GetFromCacheOrFetch()
                .ConfigureAwait(false);

            List<EventSettings<TKey>> items = allItems.Data
                .Where(x => x.EventKey == eventKey)
                .ToList();

            return items;
        }

        /// <summary>
        /// 0-based page index
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<EventSettings<TKey>>>> Select(int pageIndex, int pageSize)
        {
            TotalResult<List<EventSettings<TKey>>> allItems = await GetFromCacheOrFetch()
                .ConfigureAwait(false);

            int skip = pageIndex * pageSize;
            List<EventSettings<TKey>> selectedPage = allItems.Data
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new TotalResult<List<EventSettings<TKey>>>(selectedPage, allItems.Total);
        }

        public virtual async Task Update(List<EventSettings<TKey>> items)
        {
            await _storageQueries.Update(items).ConfigureAwait(false);
            _cache = null;
        }

        public virtual async Task Delete(List<EventSettings<TKey>> items)
        {
            await _storageQueries.Delete(items).ConfigureAwait(false);
            _cache = null;
        }


        //cache
        protected virtual async Task<TotalResult<List<EventSettings<TKey>>>> GetFromCacheOrFetch()
        {
            if (_cache != null
                && (_changeNotifier == null || _changeNotifier.HasUpdates == false))
            {
                return _cache;
            }

            TotalResult<List<EventSettings<TKey>>> allItems = await _storageQueries.Select(0, int.MaxValue)
                .ConfigureAwait(false);

            _cache = allItems;
            if (_changeNotifier != null)
            {
                _changeNotifier.StartMonitor();
            }

            return allItems;
        }
    }
}
