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
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.MongoDb.Entities;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberQueries<TDeliveryType, TCategory, TTopic> : ISubscriberQueries<ObjectId>
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>, new()
        where TCategory : SubscriberCategorySettings<ObjectId>, new()
        where TTopic : SubscriberTopicSettings<ObjectId>, new()
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSubscriberQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
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
            var pipeline = new EmptyPipelineDefinition<TTopic>()
                .As<TTopic, TTopic, TTopic>();

            var pipeline2 = pipeline.Match(ToTopicSettingsFilter(parameters, subscribersRange))
                .As<TTopic, TTopic, BsonDocument>();
            pipeline2 = AddCategoryLookupStages(pipeline2, parameters, subscribersRange);
            pipeline2 = AddDeliveryTypeLookupStages(pipeline2, parameters, subscribersRange);

            //var bsonDocs = _collectionFactory.GetCollection<TTopic>().Aggregate(pipeline2).ToList();
            //string jsonResults = bsonDocs.Select(x => x.ToJsonIntended()).Join(",");
            //return null;

            PipelineDefinition<TTopic, Subscriber<ObjectId>> pipelineProjected =
                AddSubscribersProjectionAndLimitStage(pipeline2, subscribersRange);

            return _collectionFactory.GetCollection<TTopic>()
                .Aggregate(pipelineProjected)
                .ToListAsync();
        }

        protected virtual Task<List<Subscriber<ObjectId>>> LookupStartingWithDeliveryTypes(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var pipeline = new EmptyPipelineDefinition<TDeliveryType>()
                .As<TDeliveryType, TDeliveryType, TDeliveryType>();

            var pipeline2 = pipeline.Match(ToDeliveryTypeSettingsFilter(parameters, subscribersRange))
                .As<TDeliveryType, TDeliveryType, BsonDocument>();
            pipeline2 = AddCategoryLookupStages(pipeline2, parameters, subscribersRange);

            //var bsonDocs = _collectionFactory.GetCollection<TDeliveryType>().Aggregate(pipeline2).ToListAsync();
            //string jsonResults = bsonDocs.Select(x => x.ToJsonIntended()).Join(",");
            //return null;

            PipelineDefinition<TDeliveryType, Subscriber<ObjectId>> pipelineProjected
                = AddSubscribersProjectionAndLimitStage(pipeline2, subscribersRange);

            return _collectionFactory.GetCollection<TDeliveryType>()
                .Aggregate(pipelineProjected)
                .ToListAsync();
        }

        protected virtual PipelineDefinition<TInput, BsonDocument> AddDeliveryTypeLookupStages<TInput>(
            PipelineDefinition<TInput, BsonDocument> pipeline, SubscriptionParameters parameters, 
            SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            string deliveryTypeCollection = _collectionFactory.GetCollection<TDeliveryType>().CollectionNamespace.CollectionName;
            string deliveryTypeField = FieldDefinitions.GetFieldMappedName<TTopic>(x => x.DeliveryType);
            string subscriberIdField = FieldDefinitions.GetFieldMappedName<TTopic>(x => x.SubscriberId);
            string topicLastSendDateField = FieldDefinitions.GetFieldMappedName<TTopic>(x => x.LastSendDateUtc);
            string lastVisitField = FieldDefinitions.GetFieldMappedName<TDeliveryType>(x => x.LastVisitUtc);
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

            string categoryCollection = _collectionFactory.GetCollection<TCategory>().CollectionNamespace.CollectionName;
            string subscriberIdField = subscribersRange.SelectFromTopics
                ? FieldDefinitions.GetFieldMappedName<TTopic>(x => x.SubscriberId)
                : FieldDefinitions.GetFieldMappedName<TDeliveryType>(x => x.SubscriberId);
            string deliveryTypeField = subscribersRange.SelectFromTopics
                ? FieldDefinitions.GetFieldMappedName<TTopic>(x => x.DeliveryType)
                : FieldDefinitions.GetFieldMappedName<TDeliveryType>(x => x.DeliveryType);
            string categoryField = FieldDefinitions.GetFieldMappedName<TTopic>(x => x.CategoryId);
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
            var projection = Builders<TDeliveryType>.Projection
                  .Exclude(x => x.SubscriberDeliveryTypeSettingsId)
                  .Include(x => x.SubscriberId)
                  .Include(x => x.DeliveryType)
                  .Include(x => x.Address)
                  .Include(x => x.TimeZoneId)
                  .Include(x => x.Language);

            var pipelineTyped = pipeline
                .As<TInput, BsonDocument, TDeliveryType>()
                .Project(projection)
                .As<TInput, BsonDocument, Subscriber<ObjectId>>();

            if (subscribersRange.Limit != null)
            {
                pipelineTyped = pipelineTyped.Limit(subscribersRange.Limit.Value);
            }

            return pipelineTyped;
        }



        //subscriber query filters
        public virtual FilterDefinition<TDeliveryType> ToDeliveryTypeSettingsFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<TDeliveryType>.Filter
                .Where(p => p.Address != null);

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                        && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType))
                    );
            }

            if (parameters.DeliveryType != null)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => p.DeliveryType == parameters.DeliveryType.Value);
            }
            
            if (parameters.CheckDeliveryTypeEnabled)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => p.IsEnabled == true);
            }

            if (parameters.CheckDeliveryTypeLastSendDate)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => p.LastSendDateUtc == null
                    || (p.LastVisitUtc != null && p.LastVisitUtc > p.LastSendDateUtc));
            }

            if (parameters.CheckDeliveryTypeSendCountNotGreater != null)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(
                    p => p.SendCount <= parameters.CheckDeliveryTypeSendCountNotGreater.Value);
            }

            if (parameters.CheckIsNDRBlocked)
            {
                filter &= Builders<TDeliveryType>.Filter.Where(p => p.IsNDRBlocked == false);
            }

            return filter;
        }

        protected virtual string ToDeliveryTypeSettingsLookupFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            string deliveryTypeField = FieldDefinitions.GetFieldMappedName<TDeliveryType>(x => x.DeliveryType);
            string subscriberIdField = FieldDefinitions.GetFieldMappedName<TDeliveryType>(x => x.SubscriberId);

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

        protected virtual FilterDefinition<TCategory> ToCategorySettingsFilter(
           SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<TCategory>.Filter.Where(p => true);

            if (parameters.DeliveryType != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (parameters.CategoryId != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => p.CategoryId == parameters.CategoryId);
            }

            if (parameters.CheckCategoryEnabled)
            {
                filter &= Builders<TCategory>.Filter.Where(p => p.IsEnabled == true);
            }

            if (parameters.CheckCategorySendCountNotGreater != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => p.SendCount <= parameters.CheckCategorySendCountNotGreater.Value);
            }

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => p.SubscriberId <= subscribersRange.SubscriberIdRangeToIncludingSelf.Value);
            }

            if (subscribersRange.SubscriberIdFromDeliveryTypesHandled != null
                && subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<TCategory>.Filter.Where(
                    p => p.SubscriberId != subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    || (p.SubscriberId == subscribersRange.SubscriberIdRangeToIncludingSelf.Value
                    && !subscribersRange.SubscriberIdFromDeliveryTypesHandled.Contains(p.DeliveryType)));
            }

            return filter;
        }

        protected virtual string ToCategorySettingsLookupFilter(SubscriptionParameters parameters,
           SubscribersRangeParameters<ObjectId> subscribersRange, bool joinOnCategories)
        {
            string subscriberIdField = FieldDefinitions.GetFieldMappedName<TCategory>(x => x.SubscriberId);
            string deliveryTypeField = FieldDefinitions.GetFieldMappedName<TCategory>(x => x.DeliveryType);
            string categoryField = FieldDefinitions.GetFieldMappedName<TCategory>(x => x.CategoryId);

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

        protected virtual FilterDefinition<TTopic> ToTopicSettingsFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = Builders<TTopic>.Filter.Where(
                p => p.TopicId == subscribersRange.TopicId
                && p.IsDeleted == false);

            if (parameters.DeliveryType != null)
            {
                filter &= Builders<TTopic>.Filter.Where(
                    p => p.DeliveryType == parameters.DeliveryType.Value);
            }

            if (parameters.CategoryId != null)
            {
                filter &= Builders<TTopic>.Filter.Where(
                    p => p.CategoryId == parameters.CategoryId);
            }

            if (parameters.CheckTopicEnabled)
            {
                filter &= Builders<TTopic>.Filter.Where(p => p.IsEnabled);
            }

            if (parameters.CheckTopicSendCountNotGreater != null)
            {
                filter = Builders<TTopic>.Filter.Where(
                    p => p.SendCount <= parameters.CheckTopicSendCountNotGreater.Value);
            }

            if (subscribersRange.FromSubscriberIds != null)
            {
                filter &= Builders<TTopic>.Filter.Where(
                    p => subscribersRange.FromSubscriberIds.Contains(p.SubscriberId));
            }

            if (subscribersRange.SubscriberIdRangeFromIncludingSelf != null)
            {
                filter &= Builders<TTopic>.Filter.Where(
                    p => subscribersRange.SubscriberIdRangeFromIncludingSelf.Value <= p.SubscriberId);
            }

            if (subscribersRange.SubscriberIdRangeToIncludingSelf != null)
            {
                filter &= Builders<TTopic>.Filter.Where(
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

            var operations = new List<WriteModel<TDeliveryType>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == item.ReceiverSubscriberId.Value
                    && p.DeliveryType == item.DeliveryType);

                var update = Builders<TDeliveryType>.Update.Combine();
                if (parameters.UpdateDeliveryTypeLastSendDateUtc)
                {
                    update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                }
                if (parameters.UpdateDeliveryTypeSendCount)
                {
                    update = update.Inc(p => p.SendCount, 1);
                }

                operations.Add(new UpdateOneModel<TDeliveryType>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<TDeliveryType> response =
                await _collectionFactory.GetCollection<TDeliveryType>().BulkWriteAsync(operations, options);
        }

        protected virtual async Task UpdateCategoryCounters(UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
        {
            if (!parameters.UpdateCategory)
            {
                return;
            }

            var operations = new List<WriteModel<TCategory>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<TCategory>.Filter.Where(
                    p => p.SubscriberId == item.ReceiverSubscriberId.Value
                    && p.DeliveryType == item.DeliveryType);

                var update = Builders<TCategory>.Update.Combine();
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
                    update = update.SetOnInsertAllMappedMembers(new TCategory()
                    {
                        SubscriberId = item.ReceiverSubscriberId.Value,
                        DeliveryType = item.DeliveryType,
                        CategoryId = item.CategoryId.Value,
                        LastSendDateUtc = item.SendDateUtc,
                        SendCount = 1,
                        IsEnabled = true,
                    });
                }

                operations.Add(new UpdateOneModel<TCategory>(filter, update)
                {
                    IsUpsert = parameters.CreateCategoryIfNotExist
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<TCategory> response =
                await _collectionFactory.GetCollection<TCategory>().BulkWriteAsync(operations, options);
        }

        protected virtual async Task UpdateTopicCounters(UpdateParameters parameters, List<SignalDispatch<ObjectId>> items)
        {
            if (!parameters.UpdateTopic)
            {
                return;
            }

            var operations = new List<WriteModel<TTopic>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<TTopic>.Filter.Where(
                    p => p.SubscriberId == item.ReceiverSubscriberId.Value
                    && p.CategoryId == item.CategoryId
                    && p.TopicId == item.TopicId);

                var update = Builders<TTopic>.Update.Combine();
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
                    update = update.SetOnInsertAllMappedMembers(new TTopic()
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

                operations.Add(new UpdateOneModel<TTopic>(filter, update)
                {
                    IsUpsert = parameters.CreateTopicIfNotExist
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<TTopic> response =
                await _collectionFactory.GetCollection<TTopic>().BulkWriteAsync(operations, options);
        }


    }
}
