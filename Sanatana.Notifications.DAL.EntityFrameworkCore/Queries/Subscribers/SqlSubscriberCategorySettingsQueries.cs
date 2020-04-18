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

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Queries
{
    public class SqlSubscriberCategorySettingsQueries : 
        ISubscriberCategorySettingsQueries<SubscriberCategorySettingsLong, long>
    {
        //fields
        protected ISenderDbContextFactory _dbContextFactory;
        protected SqlConnectionSettings _connectionSettings;

        
        //init
        public SqlSubscriberCategorySettingsQueries(ISenderDbContextFactory dbContextFactory,
            SqlConnectionSettings connectionSettings)
        {
            _dbContextFactory = dbContextFactory;
            _connectionSettings = connectionSettings;
        }


        //insert
        public virtual async Task Insert(List<SubscriberCategorySettingsLong> items)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<SubscriberCategorySettingsLong> command = repository.Insert<SubscriberCategorySettingsLong>();
                command.Insert.ExcludeProperty(x => x.SubscriberCategorySettingsId);
                int changes = await command.ExecuteAsync(items).ConfigureAwait(false);
            }
        }


        //select
        public virtual async Task<List<SubscriberCategorySettingsLong>> Select(List<long> subscriberIds, int categoryId)
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

            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<SubscriberCategorySettingsLong>>> Find(int pageIndex, int pageSize, bool descending)
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

            return response.ToTotalResult();
        }


        //update
        public virtual async Task UpdateIsEnabled(List<SubscriberCategorySettingsLong> items)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema
                    , TableValuedParameters.SUBSCRIBER_CATEGORY_SETTINGS_TYPE_IS_ENABLED);
                MergeCommand<SubscriberCategorySettingsLong> merge = repository.MergeTVP(items, tvpName);
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

        public virtual async Task UpsertIsEnabled(List<SubscriberCategorySettingsLong> items)
        {
            List<long> disabledCategories = items
                   .Where(x => x.IsEnabled == false)
                   .Select(x => x.SubscriberCategorySettingsId)
                   .ToList();
            List<SubscriberCategorySettingsLong > enabledCategories = items
                .Where(x => x.IsEnabled)
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
        public virtual async Task Delete(SubscriberCategorySettingsLong item)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteOneAsync(item)
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
