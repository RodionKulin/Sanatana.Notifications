using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Queries
{
    public class CachedDispatchTemplateQueries<TKey> : IDispatchTemplateQueries<TKey>
        where TKey : struct
    {
        //fields
        protected IDispatchTemplateQueries<TKey> _storageQueries;
        protected TotalResult<List<DispatchTemplate<TKey>>> _cache;
        protected IChangeNotifier<DispatchTemplate<TKey>> _changeNotifier;


        //init
        public CachedDispatchTemplateQueries(IDispatchTemplateQueries<TKey> storageQueries)
        {
            _storageQueries = storageQueries;
        }

        public CachedDispatchTemplateQueries(IDispatchTemplateQueries<TKey> storageQueries
            , IChangeNotifier<DispatchTemplate<TKey>> changeNotifier)
        {
            _storageQueries = storageQueries;
            _changeNotifier = changeNotifier;
        }



        //methods
        public virtual async Task Insert(List<DispatchTemplate<TKey>> items)
        {
            await _storageQueries.Insert(items).ConfigureAwait(false);
            _cache = null;
        }

        public virtual async Task<DispatchTemplate<TKey>> Select(TKey dispatchTemplatesId)
        {
            TotalResult<List<DispatchTemplate<TKey>>> allItems = await GetFromCacheOrFetch().ConfigureAwait(false);

            DispatchTemplate<TKey> item = allItems.Data.FirstOrDefault(
                x => EqualityComparer<TKey>.Default.Equals(x.DispatchTemplateId, dispatchTemplatesId));
            return item;
        }

        public virtual async Task<TotalResult<List<DispatchTemplate<TKey>>>> SelectPage(int page, int pageSize)
        {
            TotalResult<List<DispatchTemplate<TKey>>> allItems = await GetFromCacheOrFetch().ConfigureAwait(false);

            int skip = (page - 1) * pageSize;
            List<DispatchTemplate<TKey>> selectedPage = allItems.Data
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new TotalResult<List<DispatchTemplate<TKey>>>(selectedPage, allItems.Total);
        }

        public virtual async Task<List<DispatchTemplate<TKey>>> SelectForEventSettings(TKey eventSettingsId)
        {
            TotalResult<List<DispatchTemplate<TKey>>> allItems = await GetFromCacheOrFetch().ConfigureAwait(false);

            List<DispatchTemplate<TKey>> items = allItems.Data.Where(
                x => EqualityComparer<TKey>.Default.Equals(x.EventSettingsId, eventSettingsId))
                .ToList();

            return items;
        }

        public virtual async Task Update(List<DispatchTemplate<TKey>> items)
        {
            await _storageQueries.Update(items).ConfigureAwait(false);
            _cache = null;
        }

        public virtual async Task Delete(List<DispatchTemplate<TKey>> items)
        {
            await _storageQueries.Delete(items).ConfigureAwait(false);
            _cache = null;
        }


        //cache
        protected virtual async Task<TotalResult<List<DispatchTemplate<TKey>>>> GetFromCacheOrFetch()
        {
            if (_cache != null 
                && (_changeNotifier == null || _changeNotifier.HasUpdates == false))
            {
                return _cache;
            }

            TotalResult<List<DispatchTemplate<TKey>>> allItems = await _storageQueries.SelectPage(0, int.MaxValue)
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
