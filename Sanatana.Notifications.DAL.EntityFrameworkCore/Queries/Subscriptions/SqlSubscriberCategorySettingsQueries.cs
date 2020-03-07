using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.DAL;
using AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSubscriberCategorySettingsQueries : ISubscriberCategorySettingsQueries<long>
    {
        //fields
        protected ISenderDbContextFactory _dbContextFactory;
        protected SqlConnectionSettings _connectionSettings;
        protected IMapper _mapper;

        
        //init
        public SqlSubscriberCategorySettingsQueries(ISenderDbContextFactory dbContextFactory
            , INotificationsMapperFactory mapperFactory, SqlConnectionSettings connectionSettings)
        {
            _dbContextFactory = dbContextFactory;
            _connectionSettings = connectionSettings;
            _mapper = mapperFactory.GetMapper();
        }


        //insert
        public virtual async Task Insert(List<SubscriberCategorySettings<long>> items)
        {
            List<SubscriberCategorySettingsLong> mappedList = items
                .Select(_mapper.Map<SubscriberCategorySettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<SubscriberCategorySettingsLong> command = repository.Insert<SubscriberCategorySettingsLong>();
                command.Insert.ExcludeProperty(x => x.SubscriberCategorySettingsId);
                int changes = await command.ExecuteAsync(mappedList).ConfigureAwait(false);
            }
        }


        //select
        public virtual async Task<List<SubscriberCategorySettings<long>>> Select(List<long> subscriberIds, int categoryId)
        {
            List<SubscriberCategorySettingsLong> list = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                list = await context.SubscriberCategorySettings.Where(
                    p => subscriberIds.Contains(p.SubscriberId)
                    && p.CategoryId == categoryId)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            List<SubscriberCategorySettings<long>> result = list.Cast<SubscriberCategorySettings<long>>().ToList();
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<SubscriberCategorySettings<long>>>> Find(int pageIndex, int pageSize, bool descending)
        {
            RepositoryResult<SubscriberCategorySettingsLong> response = null;

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                response = await repository
                    .FindPageAsync<SubscriberCategorySettingsLong, long>(pageIndex, pageSize, descending
                    , x => true
                    , x => x.SubscriberCategorySettingsId, true)
                    .ConfigureAwait(false);                
            }

            return response.ToTotalResult<SubscriberCategorySettingsLong, SubscriberCategorySettings<long>>();
        }


        //update
        public virtual async Task UpdateIsEnabled(List<SubscriberCategorySettings<long>> items)
        {
            List<SubscriberCategorySettingsLong> mappedList = items
                .Select(_mapper.Map<SubscriberCategorySettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema
                    , TableValuedParameters.SUBSCRIBER_CATEGORY_SETTINGS_TYPE_IS_ENABLED);
                MergeCommand<SubscriberCategorySettingsLong> merge = repository.MergeTVP(mappedList, tvpName);
                merge.Source
                     .IncludeProperty(p => p.SubscriberCategorySettingsId)
                     .IncludeProperty(p => p.IsEnabled);
                merge.Compare
                    .IncludeProperty(p => p.SubscriberCategorySettingsId);
                merge.UpdateMatched
                    .IncludeProperty(p => p.IsEnabled);
                int changes = await merge.ExecuteAsync(MergeType.Update)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task UpsertIsEnabled(List<SubscriberCategorySettings<long>> items)
        {
            List<long> disabledCategories = items
                   .Where(x => x.IsEnabled == false)
                   .Select(x => x.SubscriberCategorySettingsId)
                   .ToList();
            List<SubscriberCategorySettingsLong > enabledCategories = items
                .Where(x => x.IsEnabled)
                .Select(_mapper.Map<SubscriberCategorySettingsLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                if(disabledCategories.Count > 0)
                {
                    int changes = await repository.DeleteManyAsync<SubscriberCategorySettingsLong>(
                        x => disabledCategories.Contains(x.SubscriberCategorySettingsId))
                        .ConfigureAwait(false);                    
                }

                if(enabledCategories.Count > 0)
                {
                    string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema
                        , TableValuedParameters.SUBSCRIBER_CATEGORY_SETTINGS_TYPE);
                    MergeCommand<SubscriberCategorySettingsLong> merge = repository.MergeTVP(enabledCategories, tvpName);

                    merge.Source
                        .IncludeProperty(p => p.SubscriberId)
                        .IncludeProperty(p => p.GroupId)
                        .IncludeProperty(p => p.DeliveryType)
                        .IncludeProperty(p => p.CategoryId)
                        .IncludeProperty(p => p.LastSendDateUtc)
                        .IncludeProperty(p => p.SendCount)
                        .IncludeProperty(p => p.IsEnabled);
                    merge.Compare
                        .IncludeProperty(p => p.SubscriberId)
                        .IncludeProperty(p => p.CategoryId)
                        .IncludeProperty(p => p.DeliveryType);
                    merge.UpdateMatched
                        .IncludeProperty(p => p.IsEnabled);
                    merge.Insert
                        .IncludeProperty(p => p.SubscriberId)
                        .IncludeProperty(p => p.GroupId)
                        .IncludeProperty(p => p.DeliveryType)
                        .IncludeProperty(p => p.CategoryId)
                        .IncludeProperty(p => p.LastSendDateUtc)
                        .IncludeProperty(p => p.SendCount)
                        .IncludeProperty(p => p.IsEnabled);

                    int changes = await merge.ExecuteAsync(MergeType.Upsert)
                        .ConfigureAwait(false);
                }
            }
        }


        //delete
        public virtual async Task Delete(SubscriberCategorySettings<long> item)
        {
            var mappedItem = _mapper.Map<SubscriberCategorySettingsLong>(item);
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteOneAsync(mappedItem)
                    .ConfigureAwait(false);
            }            
        }

        public virtual async Task Delete(long subscriberId)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SubscriberCategorySettingsLong>(
                    x => x.SubscriberId == subscriberId)
                    .ConfigureAwait(false);
            }
        }
    }
}
