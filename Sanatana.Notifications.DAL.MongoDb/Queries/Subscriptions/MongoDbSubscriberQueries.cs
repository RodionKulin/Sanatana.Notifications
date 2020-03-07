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
using System.Linq.Expressions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Sanatana.MongoDb.Extensions;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberQueries : ISubscriberQueries<ObjectId>
    {
        //fields
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberQueries(SenderMongoDbContext context)
        {
            _context = context;
        }

        

        //Select with join
        public virtual Task<List<Subscriber<ObjectId>>> Select(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            if (subscribersRange.SelectFromTopics)
            {
                return LookupStartingWithTopics(parameters, subscribersRange);
            }
            else
            {
                return LookupStartingWithDeliveryTypes(parameters, subscribersRange);
            }
        }

        protected virtual Task<List<Subscriber<ObjectId>>> LookupStartingWithTopics(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var pipeline = new EmptyPipelineDefinition<SubscriberTopicSettings<ObjectId>>()
                .As<SubscriberTopicSettings<ObjectId>, SubscriberTopicSettings<ObjectId>, SubscriberTopicSettings<ObjectId>>();

            var pipeline2 = pipeline.Match(ToTopicSettingsFilter(parameters, subscribersRange))
                .As<SubscriberTopicSettings<ObjectId>, SubscriberTopicSettings<ObjectId>, BsonDocument>();
            pipeline2 = AddCategoryLookupStages(pipeline2, parameters, subscribersRange);
            pipeline2 = AddDeliveryTypeLookupStages(pipeline2, parameters, subscribersRange);

            //var bsonDocs = _context.SubscriberTopicSettings.Aggregate(pipeline2).ToList();
            //string jsonResults = bsonDocs.Select(x => x.ToJsonIntended()).Join(",");
            //return null;

            PipelineDefinition<SubscriberTopicSettings<ObjectId>, Subscriber<ObjectId>> pipelineProjected =
                AddSubscribersProjectionAndLimitStage(pipeline2, subscribersRange);

            return _context.SubscriberTopicSettings
                .Aggregate(pipelineProjected)
                .ToListAsync();
        }

        protected virtual Task<List<Subscriber<ObjectId>>> LookupStartingWithDeliveryTypes(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var pipeline = new EmptyPipelineDefinition<SubscriberDeliveryTypeSettings<ObjectId>>()
                .As<SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>>();

            var pipeline2 = pipeline.Match(ToDeliveryTypeSettingsFilter(parameters, subscribersRange))
                .As<SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>, BsonDocument>();
            pipeline2 = AddCategoryLookupStages(pipeline2, parameters, subscribersRange);

            //var bsonDocs = _context.SubscriberDeliveryTypeSettings.Aggregate(pipeline2).ToListAsync();
            //string jsonResults = bsonDocs.Select(x => x.ToJsonIntended()).Join(",");
            //return null;

            PipelineDefinition<SubscriberDeliveryTypeSettings<ObjectId>, Subscriber<ObjectId>> pipelineProjected
                = AddSubscribersProjectionAndLimitStage(pipeline2, subscribersRange);

            return _context.SubscriberDeliveryTypeSettings
                .Aggregate(pipelineProjected)
                .ToListAsync();
        }

        protected virtual PipelineDefinition<TInput, BsonDocument> AddDeliveryTypeLookupStages<TInput>(
            PipelineDefinition<TInput, BsonDocument> pipeline, SubscriptionParameters parameters, 
            SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            string deliveryTypeCollection = _context.SubscriberDeliveryTypeSettings.CollectionNamespace.CollectionName;
            string deliveryTypeField = FieldDefinitions.GetFieldMappedName<SubscriberTopicSettings<ObjectId>>(x => x.DeliveryType);
            string subscriberIdField = FieldDefinitions.GetFieldMappedName<SubscriberTopicSettings<ObjectId>>(x => x.SubscriberId);
            string topicLastSendDateField = FieldDefinitions.GetFieldMappedName<SubscriberTopicSettings<ObjectId>>(x => x.LastSendDateUtc);
            string lastVisitField = FieldDefinitions.GetFieldMappedName<SubscriberDeliveryTypeSettings<ObjectId>>(x => x.LastVisitUtc);
            string deliveryTypeIntermediateField = "dts";

            string matchConditions = ToDeliveryTypeSettingsLookupFilter(parameters, subscribersRange);

            string deliveryTypeLookup = $@"
                {{
                    $lookup : {{ 
                        from: ""{deliveryTypeCollection}"",
                        let: {{ delivery_type: ""${deliveryTypeField}"", subscriber_id: ""${subscriberIdField}"" }},
                        pipeline: 
                        [{{ 
                            $match: {{  
                                {matchConditions}
                            }}
                        }}],
                        as: ""{deliveryTypeIntermediateField}""
                    }}
                }}";

            string hasDeliveryTypesAfterLookupStage = $@"
                {{ $match: 
                    {{ {deliveryTypeIntermediateField}: {{ $exists: true, $ne: [] }} }} 
                }}";

            string unwindStage = $"{{ $unwind: \"${deliveryTypeIntermediateField}\" }}";

            string lastSendDateMatchStage = $@"
                {{ 
                    $match: {{
                        $or: [ 
                            {{ {topicLastSendDateField}: {{ $exists: false }} }},
                            {{ 
                                $and: [
                                    {{ ""{deliveryTypeIntermediateField}.{lastVisitField}"": {{ $exists: true }} }},
                                    {{ $expr: 
                                        {{ $gt: [""{deliveryTypeIntermediateField}.{lastVisitField}"", ""{topicLastSendDateField}""] }}
                                    }}
                                ]
                            }}
                        ]  
                    }}
                }}";

            string replaceRootStage = $"{{ $replaceRoot: {{ newRoot: \"${deliveryTypeIntermediateField}\" }} }}";

            pipeline = pipeline
                .AppendStage(deliveryTypeLookup, BsonDocumentSerializer.Instance)
                .AppendStage(hasDeliveryTypesAfterLookupStage, BsonDocumentSerializer.Instance)
                .AppendStage(unwindStage, BsonDocumentSerializer.Instance);
            if (subscribersRange.SelectFromTopics && parameters.CheckTopicLastSendDate)
            {
                pipeline = pipeline.AppendStage(lastSendDateMatchStage, BsonDocumentSerializer.Instance);
            }
            return pipeline.AppendStage(replaceRootStage, BsonDocumentSerializer.Instance);
        }

        protected virtual PipelineDefinition<TInput, BsonDocument> AddCategoryLookupStages<TInput>(
            PipelineDefinition<TInput, BsonDocument> pipeline, SubscriptionParameters parameters, 
            SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            if (!subscribersRange.SelectFromCategories)
            {
                return pipeline;
            }

            string categoryCollection = _context.SubscriberCategorySettings.CollectionNamespace.CollectionName;
            string subscriberIdField = subscribersRange.SelectFromTopics
                ? FieldDefinitions.GetFieldMappedName<SubscriberTopicSettings<ObjectId>>(x => x.SubscriberId)
                : FieldDefinitions.GetFieldMappedName<SubscriberDeliveryTypeSettings<ObjectId>>(x => x.SubscriberId);
            string deliveryTypeField = subscribersRange.SelectFromTopics
                ? FieldDefinitions.GetFieldMappedName<SubscriberTopicSettings<ObjectId>>(x => x.DeliveryType)
                : FieldDefinitions.GetFieldMappedName<SubscriberDeliveryTypeSettings<ObjectId>>(x => x.DeliveryType);
            string categoryField = FieldDefinitions.GetFieldMappedName<SubscriberTopicSettings<ObjectId>>(x => x.CategoryId);
            string categoriesIntermediateField = "cats";

            bool joinOnCategories = subscribersRange.SelectFromTopics;
            string joinOnCategory = joinOnCategories
                ? $", category_id: \"{categoryField}\""
                : "";
            string matchConditions = ToCategorySettingsLookupFilter(parameters, subscribersRange, joinOnCategories);

            string categoriesLookup = $@"
                {{ 
                    $lookup : {{ 
                        from: ""{categoryCollection}"",
                        let: {{ 
                            subscriber_id: ""${subscriberIdField}"",
                            delivery_type: ""${deliveryTypeField}""
                            {joinOnCategory}
                        }},
                        pipeline: [
                            {{ 
                                $match: {{ 
                                    {matchConditions}
                                }}
                            }},
                            {{ $project: {{ _id: 1 }} }}
                        ],
                        as: ""{categoriesIntermediateField}""
                    }}
                }}";

            string hasCategoriesStage = $@"
                {{ $match: {{ 
                        {categoriesIntermediateField}: {{ $exists: true, $ne: [] }} 
                    }} 
                }}";

            return pipeline
                .AppendStage(categoriesLookup, BsonDocumentSerializer.Instance)
                .AppendStage(hasCategoriesStage, BsonDocumentSerializer.Instance);
        }

        protected virtual PipelineDefinition<TInput, Subscriber<ObjectId>> AddSubscribersProjectionAndLimitStage<TInput>(
            PipelineDefinition<TInput, BsonDocument> pipeline, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var projection = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Projection
                  .Exclude(x => x.SubscriberDeliveryTypeSettingsId)
                  .Include(x => x.SubscriberId)
                  .Include(x => x.DeliveryType)
                  .Include(x => x.Address)
                  .Include(x => x.TimeZoneId)
                  .Include(x => x.Language);

            var pipelineTyped = pipeline
                .As<TInput, BsonDocument, SubscriberDeliveryTypeSettings<ObjectId>>()
                .Project(projection)
                .As<TInput, BsonDocument, Subscriber<ObjectId>>();

            if (subscribersRange.Limit != null)
            {
                pipelineTyped = pipelineTyped.Limit(subscribersRange.Limit.Value);
            }

            return pipelineTyped;
        }



        //subscriber query filters
        public virtual FilterDefinition<SubscriberDeliveryTypeSettings<ObjectId>> ToDeliveryTypeSettingsFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter
                .Where(p => p.Address != null);

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType))
                    );
            }

            if (parameters.DeliveryType != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (subscribersRange.GroupId != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.GroupId == subscribersRange.GroupId.Value);
            }

            if (parameters.CheckDeliveryTypeEnabled)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.IsEnabled == true);
            }

            if (parameters.CheckDeliveryTypeLastSendDate)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.LastSendDateUtc == null
                    || (p.LastVisitUtc != null && p.LastVisitUtc > p.LastSendDateUtc));
            }

            if (parameters.CheckDeliveryTypeSendCountNotGreater != null)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SendCount <= parameters.CheckDeliveryTypeSendCountNotGreater.Value);
            }

            if (parameters.CheckIsNDRBlocked)
            {
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.IsNDRBlocked == false);
            }

            return filter;
        }

        protected virtual string ToDeliveryTypeSettingsLookupFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            string deliveryTypeField = FieldDefinitions.GetFieldMappedName<SubscriberDeliveryTypeSettings<ObjectId>>(x => x.DeliveryType);
            string subscriberIdField = FieldDefinitions.GetFieldMappedName<SubscriberDeliveryTypeSettings<ObjectId>>(x => x.SubscriberId);

            var deliveryTypesFilter = ToDeliveryTypeSettingsFilter(parameters, subscribersRange);
            string deliveryTypesFilterJson = FilterDefinitionExtensions.ToJson(deliveryTypesFilter);

            return $@"""$and"": 
            [
                {{ 
                    ""$expr"": {{ 
                        $and: [ 
                            {{ $eq: [""${subscriberIdField}"", ""$$subscriber_id""] }},
                            {{ $eq: [""${deliveryTypeField}"", ""$$delivery_type""] }}
                        ] 
                    }} 
                }},
                {deliveryTypesFilterJson}
            ]";
        }

        protected virtual FilterDefinition<SubscriberCategorySettings<ObjectId>> ToCategorySettingsFilter(
           SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(p => true);

            if (parameters.DeliveryType != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (parameters.CategoryId != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.CategoryId == parameters.CategoryId);
            }

            if (subscribersRange.GroupId != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.GroupId == subscribersRange.GroupId.Value);
            }

            if (parameters.CheckCategoryEnabled)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckCategorySendCountNotGreater != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SendCount <= parameters.CheckCategorySendCountNotGreater.Value);
            }

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType)));
            }

            return filter;
        }

        protected virtual string ToCategorySettingsLookupFilter(SubscriptionParameters parameters,
           SubscribersRangeParameters<ObjectId> subscribersRange, bool joinOnCategories)
        {
            string subscriberIdField = FieldDefinitions.GetFieldMappedName<SubscriberCategorySettings<ObjectId>>(x => x.SubscriberId);
            string deliveryTypeField = FieldDefinitions.GetFieldMappedName<SubscriberCategorySettings<ObjectId>>(x => x.DeliveryType);
            string categoryField = FieldDefinitions.GetFieldMappedName<SubscriberCategorySettings<ObjectId>>(x => x.CategoryId);

            string categoriesJoinCondition = joinOnCategories
                ? $", $eq: [\"${categoryField}\", \"$$category_id\"]"
                : "";

            var categoryFilter = ToCategorySettingsFilter(parameters, subscribersRange);
            string categoryFilterJson = FilterDefinitionExtensions.ToJson(categoryFilter);

            return $@"""$and"": 
            [ 
                {{ 
                    ""$expr"": {{ 
                        $and: 
                        [
                            {{ $eq: [""${ subscriberIdField }"", ""$$subscriber_id""] }},
                            {{ $eq: [""${ deliveryTypeField }"", ""$$delivery_type""] }}
                            {categoriesJoinCondition}
                        ] 
                    }} 
                }},
                {categoryFilterJson}
            ]";
        }

        protected virtual FilterDefinition<SubscriberTopicSettings<ObjectId>> ToTopicSettingsFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                p => p.TopicId == subscribersRange.TopicId
                && p.IsDeleted == false);

            if (parameters.DeliveryType != null)
            {
                filter &= Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (parameters.CategoryId != null)
            {
                filter &= Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.CategoryId == parameters.CategoryId);
            }

            if (parameters.CheckTopicEnabled)
            {
                filter &= Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => p.IsEnabled);
            }

            if (parameters.CheckTopicSendCountNotGreater != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SendCount <= parameters.CheckTopicSendCountNotGreater.Value);
            }

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter &= Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter &= Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            return filter;
        }



        //update methods
        public virtual async Task Update(UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
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

        protected virtual async Task UpdateDTCounters(UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
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

        protected virtual async Task UpdateCategoryCounters(UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
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

        protected virtual async Task UpdateTopicCounters(UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
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
