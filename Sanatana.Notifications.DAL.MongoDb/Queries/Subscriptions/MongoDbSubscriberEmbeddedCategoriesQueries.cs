using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberEmbeddedCategoriesQueries : MongoDbSubscriberQueries
    {

        //init
        public MongoDbSubscriberEmbeddedCategoriesQueries(SenderMongoDbContext context)
            : base(context)
        {
        }

        //subscribers selection
        protected override Task<List<Subscriber<ObjectId>>> LookupStartingWithDeliveryTypes(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var pipeline = new EmptyPipelineDefinition<SubscriberDeliveryTypeSettings<ObjectId>>()
                .As<SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>>();

            var pipeline2 = pipeline.Match(ToDeliveryTypeSettingsFilter(parameters, subscribersRange))
                .As<SubscriberDeliveryTypeSettings<ObjectId>, SubscriberDeliveryTypeSettings<ObjectId>, BsonDocument>();
            
            //var bsonDocs = _context.SubscriberDeliveryTypeSettings.Aggregate(pipeline2).ToListAsync();
            //string json = subscribers.Select(x => x.ToJsonIntended()).Join(",");
            //return null;

            PipelineDefinition<SubscriberDeliveryTypeSettings<ObjectId>, Subscriber<ObjectId>> pipelineProjected
                = AddSubscribersProjectionAndLimitStage(pipeline2, subscribersRange);

            return _context.SubscriberDeliveryTypeSettings
                .Aggregate(pipelineProjected)
                .ToListAsync();
        }


        //filters
        public override FilterDefinition<SubscriberDeliveryTypeSettings<ObjectId>> ToDeliveryTypeSettingsFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = base.ToDeliveryTypeSettingsFilter(parameters, subscribersRange);

            if (subscribersRange.SelectFromCategories)
            {
                var categoriesFilter = ToCategorySettingsFilter(parameters, subscribersRange);
                filter &= Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter
                    .ElemMatch(x => x.SubscriberCategorySettings, categoriesFilter);
            }

            return filter;
        }

    }
}
