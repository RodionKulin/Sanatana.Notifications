using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Queries
{
    public class CachedComposerSettingsQueries<TKey> : IComposerSettingsQueries<TKey>
        where TKey : struct
    {
        //fields
        protected IComposerSettingsQueries<TKey> _storageQueries;
        protected TotalResult<List<ComposerSettings<TKey>>> _cache;
        protected IChangeNotifier<ComposerSettings<TKey>> _changeNotifier;


        //init
        public CachedComposerSettingsQueries(IComposerSettingsQueries<TKey> storageQueries)
        {
            _storageQueries = storageQueries;
        }

        public CachedComposerSettingsQueries(IComposerSettingsQueries<TKey> storageQueries
            , IChangeNotifier<ComposerSettings<TKey>> changeNotifier)
        {
            _storageQueries = storageQueries;
            _changeNotifier = changeNotifier;
        }


        //methods
        public virtual async Task Insert(List<ComposerSettings<TKey>> items)
        {
            await _storageQueries.Insert(items).ConfigureAwait(false);
            _cache = null;
        }

        public virtual async Task<ComposerSettings<TKey>> Select(TKey composerSettingsId)
        {
            TotalResult<List<ComposerSettings<TKey>>> allItems = await GetFromCacheOrFetch()
                .ConfigureAwait(false);

            ComposerSettings<TKey> item = allItems.Data.FirstOrDefault(
                x => EqualityComparer<TKey>.Default.Equals(x.ComposerSettingsId, composerSettingsId));
            return item;
        }

        public virtual async Task<List<ComposerSettings<TKey>>> Select(int category)
        {
            TotalResult<List<ComposerSettings<TKey>>> allItems = await GetFromCacheOrFetch()
                .ConfigureAwait(false);

            List<ComposerSettings<TKey>> items = allItems.Data.Where(
                x => x.CategoryId == category)
                .ToList();

            return items;
        }

        public virtual async Task<TotalResult<List<ComposerSettings<TKey>>>> Select(int page, int pageSize)
        {
            TotalResult<List<ComposerSettings<TKey>>> allItems = await GetFromCacheOrFetch()
                .ConfigureAwait(false);

            int skip = (page - 1) * pageSize;
            List<ComposerSettings<TKey>> selectedPage = allItems.Data
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new TotalResult<List<ComposerSettings<TKey>>>(selectedPage, allItems.Total);
        }

        public virtual async Task Update(List<ComposerSettings<TKey>> items)
        {
            await _storageQueries.Update(items).ConfigureAwait(false);
            _cache = null;
        }

        public virtual async Task Delete(List<ComposerSettings<TKey>> items)
        {
            await _storageQueries.Delete(items).ConfigureAwait(false);
            _cache = null;
        }


        //cache
        protected virtual async Task<TotalResult<List<ComposerSettings<TKey>>>> GetFromCacheOrFetch()
        {
            if (_cache != null
                && (_changeNotifier == null || _changeNotifier.HasUpdates == false))
            {
                return _cache;
            }

            TotalResult<List<ComposerSettings<TKey>>> allItems = await _storageQueries.Select(1, int.MaxValue)
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
