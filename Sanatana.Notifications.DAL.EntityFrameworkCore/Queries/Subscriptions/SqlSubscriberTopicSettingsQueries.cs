using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using System.Linq.Expressions;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using LinqKit;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using Sanatana.EntityFrameworkCore.Batch;
using Microsoft.Data.SqlClient;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSubscriberTopicSettingsQueries : ISubscriberTopicSettingsQueries<long>
    {
        //fields
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;        
        protected IMapper _mapper;


        //init
        public SqlSubscriberTopicSettingsQueries(SqlConnectionSettings connectionSettings, 
            ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory)
        {
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }
        

        //insert
        public virtual async Task Insert(List<SubscriberTopicSettings<long>> items)
        {
            List<SubscriberTopicSettingsLong> mappedList = items
                .Select(_mapper.Map<SubscriberTopicSettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<SubscriberTopicSettingsLong> command = repository.Insert<SubscriberTopicSettingsLong>();
                command.Insert.ExcludeProperty(x => x.SubscriberTopicSettingsId);
                int changes = await command.ExecuteAsync(mappedList)
                    .ConfigureAwait(false);
            }
        }



        //select
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="subscriberIds"></param>
        /// <param name="deliveryTypeIds"></param>
        /// <param name="categoryIds"></param>
        /// <param name="topicIds"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<SubscriberTopicSettings<long>>>> SelectPage(int pageIndex, int pageSize,
             List<long> subscriberIds = null, List<int> deliveryTypeIds = null, List<int> categoryIds = null, List<string> topicIds = null)
        {
            RepositoryResult<SubscriberTopicSettingsLong> topicsPage;
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                Expression<Func<SubscriberTopicSettingsLong, bool>> where = x => true;
                if(subscriberIds != null)
                {
                    Expression<Func<SubscriberTopicSettingsLong, bool>> subscribersWhere = x => subscriberIds.Contains(x.SubscriberId);
                    where = where.And(subscribersWhere);
                }
                if (deliveryTypeIds != null)
                {
                    Expression<Func<SubscriberTopicSettingsLong, bool>> categoriesWhere = x => deliveryTypeIds.Contains(x.DeliveryType);
                    where = where.And(categoriesWhere);
                }
                if (categoryIds != null)
                {
                    Expression<Func<SubscriberTopicSettingsLong, bool>> categoriesWhere = x => categoryIds.Contains(x.CategoryId);
                    where = where.And(categoriesWhere);
                }
                if (topicIds != null)
                {
                    Expression<Func<SubscriberTopicSettingsLong, bool>> topicsWhere = x => topicIds.Contains(x.TopicId);
                    where = where.And(topicsWhere);
                }

                topicsPage = await repository.FindPageAsync<SubscriberTopicSettingsLong, long>(
                    pageIndex, pageSize, true, where
                    , x => x.SubscriberId, true)
                    .ConfigureAwait(false);                
            }

            return topicsPage.ToTotalResult<SubscriberTopicSettingsLong, SubscriberTopicSettings<long>>();
        }

        public virtual async Task<SubscriberTopicSettings<long>> Select(
            long subscriberId, int categoryId, string topicId)
        {
            SubscriberTopicSettings<long> item = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                item = await context.SubscriberTopicSettings.Where(
                    p => p.SubscriberId == subscriberId
                    && p.CategoryId == categoryId
                    && p.TopicId == topicId)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
            }

            return item;
        }



        //update
        public virtual async Task UpdateIsEnabled(List<SubscriberTopicSettings<long>> items)
        {
            List<SubscriberTopicSettingsLong> mappedList = items
                .Select(_mapper.Map<SubscriberTopicSettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.SUBSCRIBER_TOPIC_SETTINGS_TYPE_IS_ENABLED);
                MergeCommand<SubscriberTopicSettingsLong> merge = repository.MergeTVP(mappedList, tvpName);
                merge.Source
                     .IncludeProperty(p => p.SubscriberTopicSettingsId)
                     .IncludeProperty(p => p.IsEnabled);
                merge.Compare
                    .IncludeProperty(p => p.SubscriberTopicSettingsId);
                merge.UpdateMatched
                    .IncludeProperty(p => p.IsEnabled);

                int changes = await merge.ExecuteAsync(MergeType.Update)
                    .ConfigureAwait(false);
            }            
        }

        public virtual async Task UpsertIsEnabled(List<SubscriberTopicSettings<long>> items)
        {
            List<long> disabledTopics = items
                   .Where(x => x.IsEnabled == false)
                   .Select(x => x.SubscriberTopicSettingsId)
                   .ToList();
            List<SubscriberTopicSettingsLong> enabledTopics = items
                .Where(x => x.IsEnabled)
                .Select(_mapper.Map<SubscriberTopicSettingsLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            using (IDbContextTransaction ts = repository.Context.Database.BeginTransaction())
            {
                SqlTransaction sqlTransaction = (SqlTransaction)ts.GetDbTransaction();
                if (disabledTopics.Count > 0)
                {
                    int changes = await repository.DeleteManyAsync<SubscriberTopicSettingsLong>(
                        x => disabledTopics.Contains(x.SubscriberTopicSettingsId))
                        .ConfigureAwait(false);
                }

                if (enabledTopics.Count > 0)
                {
                    string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema
                        , TableValuedParameters.SUBSCRIBER_TOPIC_SETTINGS_TYPE);
                    MergeCommand<SubscriberTopicSettingsLong> merge = repository.MergeTVP(enabledTopics, tvpName, sqlTransaction);
                    merge.Source
                        .IncludeProperty(p => p.TopicId)
                        .IncludeProperty(p => p.CategoryId)
                        .IncludeProperty(p => p.DeliveryType)
                        .IncludeProperty(p => p.SubscriberId)
                        .IncludeProperty(p => p.AddDateUtc)
                        .IncludeProperty(p => p.IsEnabled);
                    merge.Compare
                        .IncludeProperty(p => p.SubscriberId)
                        .IncludeProperty(p => p.TopicId)
                        .IncludeProperty(p => p.CategoryId)
                        .IncludeProperty(p => p.DeliveryType);
                    merge.UpdateMatched
                        .IncludeProperty(p => p.IsEnabled);
                    merge.Insert
                        .IncludeProperty(p => p.TopicId)
                        .IncludeProperty(p => p.CategoryId)
                        .IncludeProperty(p => p.DeliveryType)
                        .IncludeProperty(p => p.SubscriberId)
                        .IncludeProperty(p => p.AddDateUtc)
                        .IncludeProperty(p => p.IsEnabled)
                        .IncludeDefaultValue(p => p.SendCount)
                        .IncludeDefaultValue(p => p.IsDeleted);
                    int changes = await merge.ExecuteAsync(MergeType.Upsert).ConfigureAwait(false);
                }

                ts.Commit();
            }
        }

        public virtual async Task Upsert(SubscriberTopicSettings<long> item, bool updateExisting)
        {
            SubscriberTopicSettingsLong mappedItem = _mapper.Map<SubscriberTopicSettingsLong>(item);
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                MergeCommand<SubscriberTopicSettingsLong> merge = repository.Merge(mappedItem);
                merge.Compare.IncludeProperty(p => p.SubscriberId)
                    .IncludeProperty(p => p.CategoryId)
                    .IncludeProperty(p => p.TopicId);
                if (updateExisting)
                {
                    merge.UpdateMatched.IncludeProperty(p => p.IsEnabled)
                        .IncludeProperty(p => p.IsDeleted);
                }
                int changes = await merge.ExecuteAsync(MergeType.Upsert)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateIsDeleted(SubscriberTopicSettings<long> item)
        {
            SubscriberTopicSettingsLong mappedItem = _mapper.Map<SubscriberTopicSettingsLong>(item);
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository
                    .UpdateOneAsync(mappedItem, x => x.IsDeleted)
                    .ConfigureAwait(false);                
            }
        }



        //delete
        public virtual async Task Delete(long subscriberId)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SubscriberTopicSettingsLong>(
                    x => x.SubscriberId == subscriberId)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task Delete(List<long> subscriberIds = null, List<int> deliveryTypeIds = null
            , List<int> categoryIds = null, List<string> topicIds = null)
        {
            Expression<Func<SubscriberTopicSettingsLong, bool>> where = x => true;
            if (subscriberIds != null)
            {
                Expression<Func<SubscriberTopicSettingsLong, bool>> subscribersWhere = x => subscriberIds.Contains(x.SubscriberId);
                where = where.And(subscribersWhere);
            }
            if (deliveryTypeIds != null)
            {
                Expression<Func<SubscriberTopicSettingsLong, bool>> categoriesWhere = x => deliveryTypeIds.Contains(x.DeliveryType);
                where = where.And(categoriesWhere);
            }
            if (categoryIds != null)
            {
                Expression<Func<SubscriberTopicSettingsLong, bool>> categoriesWhere = x => categoryIds.Contains(x.CategoryId);
                where = where.And(categoriesWhere);
            }
            if (topicIds != null)
            {
                Expression<Func<SubscriberTopicSettingsLong, bool>> topicsWhere = x => topicIds.Contains(x.TopicId);
                where = where.And(topicsWhere);
            }
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SubscriberTopicSettingsLong>(where)
                    .ConfigureAwait(false);
            }
        }

    }
}
