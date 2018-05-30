using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sanatana.MongoDb;
using System.Diagnostics;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbSubscriberQueries : ISubscriberQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //Select
        public virtual async Task<List<Subscriber<ObjectId>>> Select(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var intermediateResult = new SubscribersIntermIdiateResult<ObjectId>();

            if (subscribersRange.SelectFromTopics)
            {
                List<SubscriberTopicSettings<ObjectId>> topicSubscribers =
                    await SelectTopics(parameters, subscribersRange);
                
                subscribersRange.FromSubscriberIds = topicSubscribers.Select(p => p.SubscriberId).ToList();
                intermediateResult.TopicSubscribers = topicSubscribers;
            }

            if (subscribersRange.SelectFromCategories)
            {
                List<SubscriberCategorySettings<ObjectId>> categorySubscribers =
                    await SelectCategories(parameters, subscribersRange);
                
                subscribersRange.FromSubscriberIds = categorySubscribers.Select(p => p.SubscriberId).ToList();
                intermediateResult.CategorySubscribers = categorySubscribers;
            }

            return await SelectDeliveryTypes(parameters, subscribersRange, intermediateResult);
        }

        protected virtual async Task<List<SubscriberTopicSettings<ObjectId>>> SelectTopics(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                p => p.CategoryId == parameters.CategoryId
                && p.TopicId == subscribersRange.TopicId
                && p.IsDeleted == false);

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                        p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId)));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                        p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                        p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value));
            }

            if (parameters.CheckTopicEnabled)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => p.IsEnabled));
            }

            if (parameters.CheckTopicSendCountNotGreater != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                        p => p.SendCount <= parameters.CheckTopicSendCountNotGreater.Value));
            }

            var projection = Builders<SubscriberTopicSettings<ObjectId>>.Projection
                .Include(p => p.SubscriberId)
                .Include(p => p.CategoryId)
                .Include(p => p.TopicId)
                .Include(p => p.LastSendDateUtc);

            List<SubscriberTopicSettings<ObjectId>> list = await _context.SubscriberTopicSettings.Find(filter)
                .Project<SubscriberTopicSettings<ObjectId>>(projection)
                .Limit(subscribersRange.Limit)
                .ToListAsync();
           
            return list;
        }

        protected virtual async Task<List<SubscriberCategorySettings<ObjectId>>> SelectCategories(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(p => true);

            if (parameters.CategoryId != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => p.CategoryId == parameters.CategoryId));
            }

            if (subscribersRange.GroupId != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => p.GroupId == subscribersRange.GroupId.Value));
            }

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId)));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value));
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType))));
            }

            if (parameters.DeliveryType != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                   Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(p => p.DeliveryType == parameters.DeliveryType.Value));
            }

            if (parameters.CheckCategoryEnabled)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(p => p.IsEnabled == true));
            }

            if (parameters.CheckCategorySendCountNotGreater != null)
            {
                filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                        p => p.SendCount <= parameters.CheckCategorySendCountNotGreater.Value));
            }

            var projection = Builders<SubscriberCategorySettings<ObjectId>>.Projection
                .Include(p => p.SubscriberId)
                .Include(p => p.DeliveryType)
                .Include(p => p.CategoryId)
                .Include(p => p.LastSendDateUtc);

            List<SubscriberCategorySettings<ObjectId>> list = await _context.SubscriberCategorySettings.Find(filter)
                .Project<SubscriberCategorySettings<ObjectId>>(projection)
                .Limit(subscribersRange.Limit)
                .ToListAsync();

            return list;
        }

        protected virtual async Task<List<Subscriber<ObjectId>>> SelectDeliveryTypes(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange, SubscribersIntermIdiateResult<ObjectId> intermediateResult)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter
                    .Where(p => p.Address != null);

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId)));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value));
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                            && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType))
                        ));
            }

            if (parameters.DeliveryType != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.DeliveryType == parameters.DeliveryType.Value));
            }

            if (subscribersRange.GroupId != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.GroupId == subscribersRange.GroupId.Value));
            }

            if (parameters.CheckDeliveryTypeEnabled)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.IsEnabled == true));
            }

            if (parameters.CheckDeliveryTypeLastSendDate)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.LastSendDateUtc == null
                        || (p.LastVisitUtc != null && p.LastSendDateUtc <= p.LastVisitUtc)));
            }

            if (parameters.CheckDeliveryTypeSendCountNotGreater != null)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.SendCount <= parameters.CheckDeliveryTypeSendCountNotGreater.Value));
            }

            if (parameters.CheckIsNDRBlocked)
            {
                filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.IsNDRBlocked == false));
            }

            var projection = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Projection
                .Include(p => p.SubscriberId)
                .Include(p => p.DeliveryType)
                .Include(p => p.Address)
                .Include(p => p.Language)
                .Include(p => p.TimeZoneId)
                .Include(p => p.LastSendDateUtc);

            FindOptions options = new FindOptions()
            {
                BatchSize = subscribersRange.Limit
            };

            IFindFluent<SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>> findFluent
                = _context.SubscriberDeliveryTypeSettings.Find(filter, options)
                .Project<SubscriberDeliveryTypeSettings<ObjectId>>(projection);

            if (!parameters.CheckTopicLastSendDate &&
                !parameters.CheckCategoryLastSendDate)
            {
                findFluent = findFluent.Limit(subscribersRange.Limit);
            }

            IAsyncCursor<SubscriberDeliveryTypeSettings<ObjectId>> cursor = await findFluent.ToCursorAsync();
            List<SubscriberDeliveryTypeSettings<ObjectId>> list = await FilterDeliveryTypes(cursor, parameters, intermediateResult);

            var subscribers = list.Select(p => new Subscriber<ObjectId>()
            {
                SubscriberId = p.SubscriberId,
                DeliveryType = p.DeliveryType,
                Address = p.Address,
                TimeZoneId = p.TimeZoneId,
                Language = p.Language
            }).ToList();

            return subscribers;
        }

        protected virtual async Task<List<SubscriberDeliveryTypeSettings<ObjectId>>> FilterDeliveryTypes(
            IAsyncCursor<SubscriberDeliveryTypeSettings<ObjectId>> cursor
            , SubscriptionParameters parameters, SubscribersIntermIdiateResult<ObjectId> intermediateResult)
        {
            var list = new List<SubscriberDeliveryTypeSettings<ObjectId>>();

            while (await cursor.MoveNextAsync())
            {
                List<SubscriberDeliveryTypeSettings<ObjectId>> subscribers = cursor.Current.ToList();

                if (parameters.CheckTopicLastSendDate)
                {
                    subscribers = (from subscriber in subscribers
                             join top in intermediateResult.TopicSubscribers on
                             new
                             {
                                 SubscriberId = subscriber.SubscriberId,
                             } equals new
                             {
                                 SubscriberId = top.SubscriberId
                             }
                             where top.LastSendDateUtc == null
                                || (subscriber.LastVisitUtc != null && top.LastSendDateUtc <= subscriber.LastVisitUtc)
                             select subscriber).ToList();
                }

                if (parameters.CheckCategoryLastSendDate)
                {
                    subscribers = (from subscriber in subscribers
                             join cat in intermediateResult.CategorySubscribers on
                             new
                             {
                                 SubscriberId = subscriber.SubscriberId,
                                 DeliveryType = subscriber.DeliveryType
                             } equals new
                             {
                                 SubscriberId = cat.SubscriberId,
                                 DeliveryType = cat.DeliveryType
                             }
                             where cat.LastSendDateUtc == null
                                || (subscriber.LastVisitUtc != null && cat.LastSendDateUtc <= subscriber.LastVisitUtc)
                             select subscriber).ToList();
                }

                list.AddRange(subscribers);
            }

            return list;
        }


        //Update
        public virtual async Task Update(
           UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
        {
            if (!parameters.UpdateAnything)
            {
                return;
            }

            Task dtTask = UpdateDTCounters(parameters, items);
            Task categoryTask = UpdateCategoryCounters(parameters, items);
            Task topicTask = UpdateTopicCounters(parameters, items);

            await dtTask;
            await categoryTask;
            await topicTask;
        }

        protected virtual async Task UpdateDTCounters(
            UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
        {
            if (!parameters.UpdateDeliveryType)
            {
                return;
            }

            var operations = new List<WriteModel<SubscriberDeliveryTypeSettings<ObjectId>>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == item.ReceiverSubscriberId.Value
                    && p.DeliveryType == item.DeliveryType);

                var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update.Combine();
                if (parameters.UpdateDeliveryTypeLastSendDateUtc)
                {
                    update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                }
                if (parameters.UpdateDeliveryTypeSendCount)
                {
                    update = update.Inc(p => p.SendCount, 1);
                }

                operations.Add(new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<SubscriberDeliveryTypeSettings<ObjectId>> response =
                await _context.SubscriberDeliveryTypeSettings.BulkWriteAsync(operations, options);
        }

        protected virtual async Task UpdateCategoryCounters(
            UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
        {
            if (!parameters.UpdateCategory)
            {
                return;
            }

            var operations = new List<WriteModel<SubscriberCategorySettings<ObjectId>>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == item.ReceiverSubscriberId.Value
                    && p.DeliveryType == item.DeliveryType);

                var update = Builders<SubscriberCategorySettings<ObjectId>>.Update.Combine();
                if (parameters.UpdateDeliveryTypeLastSendDateUtc)
                {
                    update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                }
                if (parameters.UpdateDeliveryTypeSendCount)
                {
                    update = update.Inc(p => p.SendCount, 1);
                }

                if (parameters.CreateCategoryIfNotExist)
                {
                    update = update.SetOnInsertAllMappedMembers(new SubscriberCategorySettings<ObjectId>()
                    {
                        SubscriberId = item.ReceiverSubscriberId.Value,
                        DeliveryType = item.DeliveryType,
                        CategoryId = item.CategoryId.Value,
                        LastSendDateUtc = item.SendDateUtc,
                        SendCount = 1,
                        IsEnabled = true,
                    });
                }

                operations.Add(new UpdateOneModel<SubscriberCategorySettings<ObjectId>>(filter, update)
                {
                    IsUpsert = parameters.CreateCategoryIfNotExist
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<SubscriberCategorySettings<ObjectId>> response =
                await _context.SubscriberCategorySettings.BulkWriteAsync(operations, options);
        }

        protected virtual async Task UpdateTopicCounters(
            UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
        {
            if (!parameters.UpdateTopic)
            {
                return;
            }

            var operations = new List<WriteModel<SubscriberTopicSettings<ObjectId>>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == item.ReceiverSubscriberId.Value
                    && p.CategoryId == item.CategoryId
                    && p.TopicId == item.TopicId);

                var update = Builders<SubscriberTopicSettings<ObjectId>>.Update.Combine();
                if (parameters.UpdateDeliveryTypeLastSendDateUtc)
                {
                    update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                }
                if (parameters.UpdateDeliveryTypeSendCount)
                {
                    update = update.Inc(p => p.SendCount, 1);
                }

                if (parameters.CreateCategoryIfNotExist)
                {
                    update = update.SetOnInsertAllMappedMembers(new SubscriberTopicSettings<ObjectId>()
                    {
                        SubscriberId = item.ReceiverSubscriberId.Value,
                        CategoryId = item.CategoryId.Value,
                        TopicId = item.TopicId,
                        LastSendDateUtc = item.SendDateUtc,
                        SendCount = 1,
                        AddDateUtc = DateTime.UtcNow,
                        IsEnabled = true,
                        IsDeleted = false
                    });
                }

                operations.Add(new UpdateOneModel<SubscriberTopicSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = parameters.CreateTopicIfNotExist
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<SubscriberTopicSettings<ObjectId>> response =
                await _context.SubscriberTopicSettings.BulkWriteAsync(operations, options);
        }

    }
}
