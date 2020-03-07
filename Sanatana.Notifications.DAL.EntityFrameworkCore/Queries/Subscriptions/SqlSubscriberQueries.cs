using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSubscriberQueries : ISubscriberQueries<long>
    {
        //fields        
        protected ISenderDbContextFactory _dbContextFactory;
        protected SqlConnectionSettings _connectionSettings;


        //init
        public SqlSubscriberQueries(SqlConnectionSettings connectionSettings, ISenderDbContextFactory dbContextFactory)
        {
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
        }



        //select
        public virtual async Task<List<Subscriber<long>>> Select(
            SubscriptionParameters parameters, SubscribersRangeParameters<long> subscribersRange)
        {
            List<Subscriber<long>> subscribers = null;
            
            using (var context = _dbContextFactory.GetDbContext())
            {
                IQueryable<Subscriber<long>> query =
                    CreateSelectQuery(parameters, subscribersRange, context);

                subscribers = await query.ToListAsync().ConfigureAwait(false);
            }
            
            return subscribers;
        }

        public virtual IQueryable<Subscriber<long>> CreateSelectQuery(SubscriptionParameters parameters
            , SubscribersRangeParameters<long> subscribersRange, SenderDbContext context)
        {
            IQueryable<SubscriberDeliveryTypeSettingsLong> query =
                CreateDeliveryTypeSelectQuery(parameters, subscribersRange, context);

            if (subscribersRange.SelectFromCategories)
            {
                IQueryable<SubscriberCategorySettingsLong> categoryQueryPart = CreateCategorySelectQuery(parameters, subscribersRange, context);
                query = JoinWithCategoriesSelect(parameters, query, categoryQueryPart);
            }

            if (subscribersRange.SelectFromTopics)
            {
                IQueryable<SubscriberTopicSettingsLong> topicQueryPart = CreateTopicSelectQuery(parameters, subscribersRange, context);
                query = JoinWithTopicsSelect(parameters, subscribersRange, query, topicQueryPart);
            }

            IQueryable<Subscriber<long>> subscribers = query.Select(d => new Subscriber<long>
            {
                SubscriberId = d.SubscriberId,
                DeliveryType = d.DeliveryType,
                Address = d.Address,
                TimeZoneId = d.TimeZoneId,
                Language = d.Language
            });

            if (subscribersRange.Limit != null)
            {
                subscribers = subscribers.Take(subscribersRange.Limit.Value);
            }

            return subscribers;
        }

        protected virtual IQueryable<SubscriberDeliveryTypeSettingsLong> CreateDeliveryTypeSelectQuery(
            SubscriptionParameters parameters, SubscribersRangeParameters<long> subscribersRange, SenderDbContext context)
        {
            IQueryable<SubscriberDeliveryTypeSettingsLong> query = context.SubscriberDeliveryTypeSettings
                .Where(p => p.Address != null);

            if (subscribersRange.FromSubscriberIds != null)
            {
                query = query.Where(p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                query = query.Where(p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                query = query.Where(p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                query = query.Where(
                    p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType))
                    );
            }

            if (parameters.DeliveryType != null)
            {
                query = query.Where(p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (subscribersRange.GroupId != null)
            {
                query = query.Where(p => p.GroupId == subscribersRange.GroupId.Value);
            }

            if (parameters.CheckDeliveryTypeEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckDeliveryTypeLastSendDate)
            {
                query = query.Where(p => p.LastSendDateUtc == null
                    || (p.LastVisitUtc != null && p.LastSendDateUtc < p.LastVisitUtc));
            }

            if (parameters.CheckDeliveryTypeSendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckDeliveryTypeSendCountNotGreater.Value;
                query = query.Where(p => p.SendCount <= sendCountLimitValue);
            }

            if (parameters.CheckIsNDRBlocked)
            {
                query = query.Where(p => p.IsNDRBlocked == false);
            }

            return query;
        }

        protected virtual IQueryable<SubscriberCategorySettingsLong> CreateCategorySelectQuery(
            SubscriptionParameters parameters, SubscribersRangeParameters<long> subscribersRange, SenderDbContext context)
        {
            IQueryable<SubscriberCategorySettingsLong> query = context.SubscriberCategorySettings;

            if (parameters.CategoryId != null)
            {
                query = query.Where(p => p.CategoryId == parameters.CategoryId);
            }

            if (parameters.DeliveryType != null)
            {
                query = query.Where(p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (subscribersRange.GroupId != null)
            {
                query = query.Where(p => p.GroupId == subscribersRange.GroupId.Value);
            }

            if (subscribersRange.FromSubscriberIds != null)
            {
                query = query.Where(p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                query = query.Where(p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                query = query.Where(p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                query = query.Where(
                    p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType))
                    );
            }

            if (parameters.CheckCategoryEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckCategorySendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckCategorySendCountNotGreater.Value;
                query = query.Where(p => p.SendCount <= sendCountLimitValue);
            }

            return query;
        }

        protected virtual IQueryable<SubscriberDeliveryTypeSettingsLong> JoinWithCategoriesSelect(
            SubscriptionParameters parameters, IQueryable<SubscriberDeliveryTypeSettingsLong> typeQueryPart
            , IQueryable<SubscriberCategorySettingsLong> categoryQueryPart)
        {
            int categoryIdValue = parameters.CategoryId.Value;

            IQueryable<SubscriberDeliveryTypeSettingsLong> query = null;
           
            if (parameters.CheckCategoryLastSendDate)
            {
                query = from d in typeQueryPart
                        join c in categoryQueryPart on
                        new
                        {
                            SubscriberId = d.SubscriberId,
                            DeliveryType = d.DeliveryType,
                            CategoryId = categoryIdValue
                        } equals new
                        {
                            SubscriberId = c.SubscriberId,
                            DeliveryType = c.DeliveryType,
                            CategoryId = c.CategoryId
                        }
                        into gr
                        from catGroup in gr.DefaultIfEmpty()
                        where catGroup != null
                        && (catGroup.LastSendDateUtc == null
                            || (d.LastVisitUtc != null && catGroup.LastSendDateUtc < d.LastVisitUtc))
                        select d;
            }
            else
            {
                query = from d in typeQueryPart
                        join c in categoryQueryPart on
                        new
                        {
                            SubscriberId = d.SubscriberId,
                            DeliveryType = d.DeliveryType,
                            CategoryId = categoryIdValue
                        } equals new
                        {
                            SubscriberId = c.SubscriberId,
                            DeliveryType = c.DeliveryType,
                            CategoryId = c.CategoryId
                        }
                        into gr
                        from catGroup in gr.DefaultIfEmpty()
                        select d;
            }

            return query;
        }

        protected virtual IQueryable<SubscriberTopicSettingsLong> CreateTopicSelectQuery(
            SubscriptionParameters parameters, SubscribersRangeParameters<long> subscribersRange, SenderDbContext context)
        {
            IQueryable<SubscriberTopicSettingsLong> query = context.SubscriberTopicSettings
                .Where(p => p.TopicId == subscribersRange.TopicId
                && p.CategoryId == parameters.CategoryId.Value
                && p.IsDeleted == false);

            if (subscribersRange.FromSubscriberIds != null)
            {
                query = query.Where(p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (parameters.DeliveryType != null)
            {
                query = query.Where(p => p.DeliveryType == parameters.DeliveryType.Value);
            }
            
            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                query = query.Where(p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                query = query.Where(p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (parameters.CheckTopicEnabled)
            {
                query = query.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckTopicSendCountNotGreater != null)
            {
                int sendCountLimitValue = parameters.CheckTopicSendCountNotGreater.Value;
                query = query.Where(p => p.SendCount <= sendCountLimitValue);
            }

            return query;
        }

        protected virtual IQueryable<SubscriberDeliveryTypeSettingsLong> JoinWithTopicsSelect(SubscriptionParameters parameters, SubscribersRangeParameters<long> subscribersRange
            , IQueryable<SubscriberDeliveryTypeSettingsLong> typeQueryPart, IQueryable<SubscriberTopicSettingsLong> topicQueryPart)
        {
            int categoryIdValue = parameters.CategoryId.Value;
            string topicIdValue = subscribersRange.TopicId ?? parameters.TopicId;
      
            IQueryable<SubscriberDeliveryTypeSettingsLong> query = null;

            if (parameters.CheckTopicLastSendDate)
            {
                query = from d in typeQueryPart
                        join t in topicQueryPart on
                        new
                        {
                            SubscriberId = d.SubscriberId,
                            CategoryId = categoryIdValue,
                            DeliveryType = d.DeliveryType,
                            TopicId = topicIdValue
                        } equals new
                        {
                            SubscriberId = t.SubscriberId,
                            CategoryId = t.CategoryId,
                            DeliveryType = t.DeliveryType,
                            TopicId = t.TopicId
                        }
                        where (t.LastSendDateUtc == null
                        || (d.LastVisitUtc != null
                            && t.LastSendDateUtc < d.LastVisitUtc))
                        select d;
            }
            else
            {
                query = from d in typeQueryPart
                        join t in topicQueryPart on
                        new
                        {
                            SubscriberId = d.SubscriberId,
                            CategoryId = categoryIdValue,
                            DeliveryType = d.DeliveryType,
                            TopicId = topicIdValue
                        } equals new
                        {
                            SubscriberId = t.SubscriberId,
                            CategoryId = t.CategoryId,
                            DeliveryType = t.DeliveryType,
                            TopicId = t.TopicId
                        }
                        select d;
            }

            return query;
        }



        //update
        public virtual async Task Update(UpdateParameters parameters, List<SignalDispatch<long>> items)
        {
            if (!parameters.UpdateAnything)
            {
                return;
            }
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                SqlParameter updateSubscribersParam = ToUpdateSubscriberType(items);
                string command = CreateUpdateQuery(parameters, context);

                int changes = await context.Database.ExecuteSqlCommandAsync(command, updateSubscribersParam)
                    .ConfigureAwait(false);
            }
        }

        protected virtual SqlParameter ToUpdateSubscriberType(List<SignalDispatch<long>> items)
        {
            IEnumerable<UpdateSubscriberParameter> groupedItems = items
                .Where(p => p.ReceiverSubscriberId != null)
                .GroupBy(p => new { p.ReceiverSubscriberId, p.DeliveryType, p.CategoryId, p.TopicId })
                .Select(p => new UpdateSubscriberParameter
                {
                    SubscriberId = p.First().ReceiverSubscriberId.Value,
                    DeliveryType = p.First().DeliveryType,
                    CategoryId = p.First().CategoryId.Value,
                    TopicId = p.First().TopicId,
                    SendCount = p.Count()
                });
            
            DataTable dataTable = groupedItems.ToDataTable(new List<string>()
            {
                nameof(SubscriberDeliveryTypeSettingsLong.SubscriberId),
                nameof(SubscriberDeliveryTypeSettingsLong.SendCount),
                nameof(SubscriberDeliveryTypeSettingsLong.DeliveryType),
                nameof(SubscriberTopicSettingsLong.CategoryId),
                nameof(SubscriberTopicSettingsLong.TopicId),
            });

            SqlParameter param = new SqlParameter(
                TableValuedParameters.UPDATE_SUBSCRIBERS_PARAMETER_NAME, dataTable);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.SUBSCRIBER_TYPE);

            return param;
        }

        protected virtual string CreateUpdateQuery(UpdateParameters parameters, DbContext context)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            if (parameters.UpdateDeliveryType)
            {
                scriptBuilder.AppendLine(
                    CreateDeliveryTypeUpdateQuery(parameters, context));
                scriptBuilder.AppendLine();
            }

            if (parameters.UpdateCategory)
            {
                scriptBuilder.AppendLine(
                    CreateCategoryUpdateQuery(parameters, context));
                scriptBuilder.AppendLine();
            }

            if (parameters.UpdateTopic)
            {
                scriptBuilder.AppendLine(
                    CreateTopicUpdateQuery(parameters, context));
                scriptBuilder.AppendLine();
            }

            return scriptBuilder.ToString();
        }

        protected virtual string CreateDeliveryTypeUpdateQuery(UpdateParameters parameters, DbContext context)
        {
            string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.SUBSCRIBER_TYPE);
            var merge = new MergeCommand<SubscriberDeliveryTypeSettingsLong>(context, null, tvpName, TableValuedParameters.UPDATE_SUBSCRIBERS_PARAMETER_NAME);

            merge.Compare
                .IncludeProperty(p => p.SubscriberId)
                .IncludeProperty(p => p.DeliveryType);

            merge.UpdateMatched.ExcludeAllByDefault = true;

            if (parameters.UpdateDeliveryTypeSendCount)
            {
                merge.UpdateMatched.Assign(t => t.SendCount, (t, s) => t.SendCount + s.SendCount);
            }

            if (parameters.UpdateDeliveryTypeLastSendDateUtc)
            {
                merge.UpdateMatched.Assign(t => t.LastSendDateUtc, (t, s) => DateTime.UtcNow);
            }

            return merge.ConstructCommandTVP(MergeType.Update);
        }

        protected virtual string CreateCategoryUpdateQuery(UpdateParameters parameters, DbContext context)
        {
            string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.SUBSCRIBER_TYPE);
            var merge = new MergeCommand<SubscriberCategorySettingsLong>(context, null, tvpName, TableValuedParameters.UPDATE_SUBSCRIBERS_PARAMETER_NAME);

            merge.Compare
                .IncludeProperty(p => p.SubscriberId)
                .IncludeProperty(p => p.DeliveryType)
                .IncludeProperty(p => p.CategoryId);

            merge.UpdateMatched.ExcludeAllByDefault = true;

            if (parameters.UpdateCategorySendCount)
            {
                merge.UpdateMatched.Assign(t => t.SendCount, (t, s) => t.SendCount + s.SendCount);
            }

            if (parameters.UpdateCategoryLastSendDateUtc)
            {
                merge.UpdateMatched.Assign(t => t.LastSendDateUtc, (t, s) => DateTime.UtcNow);
            }

            if (parameters.CreateCategoryIfNotExist)
            {
                merge.Insert
                    .ExcludeProperty(t => t.SubscriberCategorySettingsId)
                    .IncludeValue(t => t.IsEnabled, true)
                    .IncludeValue(t => t.GroupId, null)
                    .IncludeValue(t => t.LastSendDateUtc, DateTime.UtcNow);
            }

            MergeType mergeType = parameters.CreateCategoryIfNotExist
                ? MergeType.Upsert
                : MergeType.Update;
            return merge.ConstructCommandTVP(mergeType);
        }

        protected virtual string CreateTopicUpdateQuery(UpdateParameters parameters, DbContext context)
        {
            string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.SUBSCRIBER_TYPE);
            var merge = new MergeCommand<SubscriberTopicSettingsLong>(context, null, tvpName, TableValuedParameters.UPDATE_SUBSCRIBERS_PARAMETER_NAME);

            merge.Compare.IncludeProperty(p => p.SubscriberId)
                .IncludeProperty(p => p.CategoryId)
                .IncludeProperty(p => p.TopicId);

            merge.UpdateMatched.ExcludeAllByDefault = true;

            if (parameters.UpdateTopicSendCount)
            {
                merge.UpdateMatched.Assign(t => t.SendCount, (t, s) => t.SendCount + s.SendCount);
            }

            if (parameters.UpdateTopicLastSendDateUtc)
            {
                merge.UpdateMatched.Assign(t => t.LastSendDateUtc, (t, s) => DateTime.UtcNow);
            }

            if (parameters.CreateTopicIfNotExist)
            {
                merge.Insert
                    .ExcludeProperty(t => t.SubscriberTopicSettingsId)
                    .IncludeValue(t => t.AddDateUtc, DateTime.UtcNow)
                    .IncludeValue(t => t.IsEnabled, true)
                    .IncludeValue(t => t.LastSendDateUtc, DateTime.UtcNow)
                    .IncludeValue(t => t.IsDeleted, false);
            }

            MergeType mergeType = parameters.CreateTopicIfNotExist
                ? MergeType.Upsert
                : MergeType.Update;
            return merge.ConstructCommandTVP(mergeType);
        }

    }
}
