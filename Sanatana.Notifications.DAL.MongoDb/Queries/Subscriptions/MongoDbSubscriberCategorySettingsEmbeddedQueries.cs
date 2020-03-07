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

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberCategorySettingsEmbeddedQueries : ISubscriberCategorySettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;

        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberCategorySettingsEmbeddedQueries(MongoDbConnectionSettings connectionSettings)
        {

            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual Task Insert(List<SubscriberCategorySettings<ObjectId>> settings)
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

            var writeOperations = new List<WriteModel<SubscriberDeliveryTypeSettings<ObjectId>>>();
            foreach (var deliveryTypeGroup in deliveryTypeGroups)
            {
                var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == deliveryTypeGroup.Key.SubscriberId
                    && p.DeliveryType == deliveryTypeGroup.Key.DeliveryType);

                var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                    .AddToSetEach(x => x.SubscriberCategorySettings, deliveryTypeGroup);

                var updateModel = new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update); ;
                writeOperations.Add(updateModel);
            }

           return _context.SubscriberDeliveryTypeSettings
                .BulkWriteAsync(writeOperations, new BulkWriteOptions
                {
                    IsOrdered = false
                });
        }

        public virtual async Task<List<SubscriberCategorySettings<ObjectId>>> Select(
            List<ObjectId> subscriberIds, int categoryId)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                   p => subscriberIds.Contains(p.SubscriberId));

            var projection = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Projection
                .Include(x => x.SubscriberCategorySettings);

            List<SubscriberDeliveryTypeSettings<ObjectId>> deliveryTypes = await _context.SubscriberDeliveryTypeSettings
                .Find(filter)
                .Project<SubscriberDeliveryTypeSettings<ObjectId>>(projection)
                .ToListAsync();

            List<SubscriberCategorySettings<ObjectId>> categorySettings = deliveryTypes
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
        public virtual async Task<TotalResult<List<SubscriberCategorySettings<ObjectId>>>> Find(
            int pageIndex, int pageSize, bool descending)
        {
            //select categories
            var pipeline = new EmptyPipelineDefinition<SubscriberDeliveryTypeSettings<ObjectId>>();

            var pipeline2 = pipeline.Unwind(x => x.SubscriberCategorySettings)
                .As<SubscriberDeliveryTypeSettings<ObjectId>, BsonDocument, SubscriberDeliveryTypeSettings<ObjectId>>();

            var pipeline3 = pipeline2.ReplaceRoot(x => x.SubscriberCategorySettings)
                .As<SubscriberDeliveryTypeSettings<ObjectId>, List<SubscriberCategorySettings<ObjectId>>, SubscriberCategorySettings<ObjectId>>();

            //count total categories
            var countPipeline = new EmptyPipelineDefinition<SubscriberCategorySettings<ObjectId>>().Count();
            var countFacetStage = AggregateFacet.Create("count", countPipeline);

            //limit page of categories
            var limitPipeline = new EmptyPipelineDefinition<SubscriberCategorySettings<ObjectId>>()
                .As<SubscriberCategorySettings<ObjectId>, SubscriberCategorySettings<ObjectId>, SubscriberCategorySettings<ObjectId>>();
            var sort = Builders<SubscriberCategorySettings<ObjectId>>.Sort.Combine();
            sort = descending
                ? sort.Descending(x => x.SubscriberCategorySettingsId)
                : sort.Ascending(x => x.SubscriberCategorySettingsId);
            limitPipeline = limitPipeline.Sort(sort);

            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            limitPipeline = limitPipeline.Skip(skip);
            limitPipeline = limitPipeline.Limit(pageSize);
            var dataFacetStage = AggregateFacet.Create("data", limitPipeline);

            //combine facets
            var pipeline4 = pipeline3.Facet(new AggregateFacet<SubscriberCategorySettings<ObjectId>>[] {
                countFacetStage,
                dataFacetStage
            });

            //query
            IAsyncCursor<AggregateFacetResults> categorySettings = await _context.SubscriberDeliveryTypeSettings
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
            var categories = dataFacetResult.Output<SubscriberCategorySettings<ObjectId>>();

            return new TotalResult<List<SubscriberCategorySettings<ObjectId>>>(
                categories.ToList(),
                count.Count);
        }

        public virtual Task UpdateIsEnabled(List<SubscriberCategorySettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<SubscriberDeliveryTypeSettings<ObjectId>>>();

            foreach (SubscriberCategorySettings<ObjectId> item in items)
            {
                var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter
                    .Where(p => p.SubscriberId == item.SubscriberId
                    && p.DeliveryType == item.DeliveryType);
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter
                    .ElemMatch(x => x.SubscriberCategorySettings,
                    cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

                var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.SubscriberCategorySettings[-1].IsEnabled, item.IsEnabled);

                requests.Add(new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            return _context.SubscriberDeliveryTypeSettings.BulkWriteAsync(requests, options);
        }

        public virtual Task UpsertIsEnabled(List<SubscriberCategorySettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<SubscriberDeliveryTypeSettings<ObjectId>>>();

            foreach (SubscriberCategorySettings<ObjectId> item in items)
            {
                var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == item.SubscriberId
                    && p.DeliveryType == item.DeliveryType);

                if (item.IsEnabled)
                {
                    filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.ElemMatch(
                        x => x.SubscriberCategorySettings,
                        cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);
                    var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                        .Set(p => p.SubscriberCategorySettings[-1].IsEnabled, item.IsEnabled);
                    requests.Add(new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
                else
                {
                    var pullFilter = Builders<SubscriberCategorySettings<ObjectId>>.Filter
                        .Where(cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);
                    var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                        .PullFilter(x => x.SubscriberCategorySettings, pullFilter);
                    requests.Add(new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }
            }

            return _context.SubscriberDeliveryTypeSettings.BulkWriteAsync(requests, new BulkWriteOptions
            {
                IsOrdered = false
            });
        }

        public virtual Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                p => p.SubscriberId == subscriberId);

            var pullFilter = Builders<SubscriberCategorySettings<ObjectId>>.Filter
                .Where(cat => true);
            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .PullFilter(x => x.SubscriberCategorySettings, pullFilter);

            return _context.SubscriberDeliveryTypeSettings.UpdateManyAsync(filter, update, new UpdateOptions
            {
                IsUpsert = false,
            });
        }

        public virtual Task Delete(SubscriberCategorySettings<ObjectId> item)
        {
            var requests = new List<WriteModel<SubscriberDeliveryTypeSettings<ObjectId>>>();

            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                p => p.SubscriberId == item.SubscriberId
                && p.DeliveryType == item.DeliveryType);

            var pullFilter = Builders<SubscriberCategorySettings<ObjectId>>.Filter
                .Where(cat => cat.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);
            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .PullFilter(x => x.SubscriberCategorySettings, pullFilter);
            requests.Add(new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update)
            {
                IsUpsert = false
            });

            return _context.SubscriberDeliveryTypeSettings.BulkWriteAsync(requests, new BulkWriteOptions
            {
                IsOrdered = false
            });
        }



    }
}
