using MongoDB.Bson;
using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.MongoDb.Entities;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberCategorySettingsEmbeddedQueries<TCategory, TDeliveryType> : ISubscriberCategorySettingsQueries<TCategory, ObjectId>
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>
        where TCategory : SubscriberCategorySettings<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSubscriberCategorySettingsEmbeddedQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }



        //methods
        public virtual Task Insert(List<TCategory> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return Task.CompletedTask;
            }

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            var deliveryTypeGroups = settings
                .GroupBy(x => new { x.SubscriberId, x.DeliveryType });

            var writeOperations = new List<WriteModel<TDeliveryType>>();
            foreach (var deliveryTypeGroup in deliveryTypeGroups)
            {
                var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == deliveryTypeGroup.Key.SubscriberId
                    && p.DeliveryType == deliveryTypeGroup.Key.DeliveryType);

                var update = Builders<TDeliveryType>.Update
                    .AddToSetEach(x => x.SubscriberCategorySettings, deliveryTypeGroup);

                var updateModel = new UpdateOneModel<TDeliveryType>(filter, update); ;
                writeOperations.Add(updateModel);
            }

           return _collectionFactory
                .GetCollection<TDeliveryType>()
                .BulkWriteAsync(writeOperations, new BulkWriteOptions
                {
                    IsOrdered = false
                });
        }

        public virtual async Task<List<TCategory>> Select(List<ObjectId> subscriberIds, int categoryId)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                   p => subscriberIds.Contains(p.SubscriberId));

            var projection = Builders<TDeliveryType>.Projection
                .Include(x => x.SubscriberCategorySettings);

            List<TDeliveryType> deliveryTypes = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .Find(filter)
                .Project<TDeliveryType>(projection)
                .ToListAsync()
                .ConfigureAwait(false);

            List<TCategory> categorySettings = deliveryTypes
                .SelectMany(x => x.SubscriberCategorySettings)
                .Where(x => x.CategoryId == categoryId)
                .ToList();

            return categorySettings;
        }

        /// <summary>
        /// Get page of category settings
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<TCategory>>> Find(
            int pageIndex, int pageSize, bool descending)
        {
            //select categories
            var pipeline = new EmptyPipelineDefinition<TDeliveryType>();

            var pipeline2 = pipeline.Unwind(x => x.SubscriberCategorySettings)
                .As<TDeliveryType, BsonDocument, TDeliveryType>();

            var pipeline3 = pipeline2.ReplaceRoot(x => x.SubscriberCategorySettings)
                .As<TDeliveryType, List<TCategory>, TCategory>();

            //count total categories
            var countPipeline = new EmptyPipelineDefinition<TCategory>().Count();
            var countFacetStage = AggregateFacet.Create("count", countPipeline);

            //limit page of categories
            var limitPipeline = new EmptyPipelineDefinition<TCategory>()
                .As<TCategory, TCategory, TCategory>();
            var sort = Builders<TCategory>.Sort.Combine();
            sort = descending
                ? sort.Descending(x => x.SubscriberCategorySettingsId)
                : sort.Ascending(x => x.SubscriberCategorySettingsId);
            limitPipeline = limitPipeline.Sort(sort);

            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            limitPipeline = limitPipeline.Skip(skip);
            limitPipeline = limitPipeline.Limit(pageSize);
            var dataFacetStage = AggregateFacet.Create("data", limitPipeline);

            //combine facets
            var pipeline4 = pipeline3.Facet(new AggregateFacet<TCategory>[] {
                countFacetStage,
                dataFacetStage
            });

            //query
            IAsyncCursor<AggregateFacetResults> categorySettings = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .AggregateAsync(pipeline4, new AggregateOptions())
                .ConfigureAwait(false);

            List<AggregateFacetResults> aggregateFacetResults = await categorySettings
                .ToListAsync()
                .ConfigureAwait(false);

            AggregateFacetResult countFacetResult = aggregateFacetResults[0].Facets
                .First(x => x.Name == "count");
            AggregateFacetResult dataFacetResult = aggregateFacetResults[0].Facets
                .First(x => x.Name == "data");

            var count = countFacetResult.Output<AggregateCountResult>();
            var categories = dataFacetResult.Output<TCategory>();

            return new TotalResult<List<TCategory>>(
                categories.ToList(),
                count.Count);
        }

        public virtual Task UpdateIsEnabled(List<TCategory> items)
        {
            var requests = new List<WriteModel<TDeliveryType>>();

            foreach (TCategory item in items)
            {
                var filter = Builders<TDeliveryType>.Filter
                    .Where(p => p.SubscriberId == item.SubscriberId
                    && p.DeliveryType == item.DeliveryType);
                filter &= Builders<TDeliveryType>.Filter
                    .ElemMatch(x => x.SubscriberCategorySettings,
                    cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

                var update = Builders<TDeliveryType>.Update
                    .Set(p => p.SubscriberCategorySettings[-1].IsEnabled, item.IsEnabled);

                requests.Add(new UpdateOneModel<TDeliveryType>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            return _collectionFactory
                .GetCollection<TDeliveryType>()
                .BulkWriteAsync(requests, options);
        }

        public virtual Task UpsertIsEnabled(List<TCategory> items)
        {
            var requests = new List<WriteModel<TDeliveryType>>();

            foreach (TCategory item in items)
            {
                var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == item.SubscriberId
                    && p.DeliveryType == item.DeliveryType);

                if (item.IsEnabled)
                {
                    filter &= Builders<TDeliveryType>.Filter.ElemMatch(
                        x => x.SubscriberCategorySettings,
                        cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);
                    var update = Builders<TDeliveryType>.Update
                        .Set(p => p.SubscriberCategorySettings[-1].IsEnabled, item.IsEnabled);
                    requests.Add(new UpdateOneModel<TDeliveryType>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
                else
                {
                    var pullFilter = Builders<TCategory>.Filter
                        .Where(cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);
                    var update = Builders<TDeliveryType>.Update
                        .PullFilter(x => x.SubscriberCategorySettings, pullFilter);
                    requests.Add(new UpdateOneModel<TDeliveryType>(filter, update)
                    {
                        IsUpsert = false
                    });
                }
            }

            return _collectionFactory
                .GetCollection<TDeliveryType>()
                .BulkWriteAsync(requests, new BulkWriteOptions
                {
                    IsOrdered = false
                });
        }

        public virtual Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                p => p.SubscriberId == subscriberId);

            var pullFilter = Builders<TCategory>.Filter
                .Where(cat => true);
            var update = Builders<TDeliveryType>.Update
                .PullFilter(x => x.SubscriberCategorySettings, pullFilter);

            return _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateManyAsync(filter, update, new UpdateOptions
                {
                    IsUpsert = false,
                });
        }

        public virtual Task Delete(TCategory item)
        {
            var requests = new List<WriteModel<TDeliveryType>>();

            var filter = Builders<TDeliveryType>.Filter.Where(
                p => p.SubscriberId == item.SubscriberId
                && p.DeliveryType == item.DeliveryType);

            var pullFilter = Builders<TCategory>.Filter
                .Where(cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);
            var update = Builders<TDeliveryType>.Update
                .PullFilter(x => x.SubscriberCategorySettings, pullFilter);
            requests.Add(new UpdateOneModel<TDeliveryType>(filter, update)
            {
                IsUpsert = false
            });

            return _collectionFactory
                .GetCollection<TDeliveryType>()
                .BulkWriteAsync(requests, new BulkWriteOptions
                {
                    IsOrdered = false
                });
        }



    }
}
