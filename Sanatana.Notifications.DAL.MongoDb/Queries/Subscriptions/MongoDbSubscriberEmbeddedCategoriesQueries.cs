using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.MongoDb.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberEmbeddedCategoriesQueries<TDeliveryType, TCategory, TTopic> : MongoDbSubscriberQueries<TDeliveryType, TCategory, TTopic>
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>, new()
        where TCategory : SubscriberCategorySettings<ObjectId>, new()
        where TTopic : SubscriberTopicSettings<ObjectId>, new()
    {

        //init
        public MongoDbSubscriberEmbeddedCategoriesQueries(ICollectionFactory collectionFactory)
            : base(collectionFactory)
        {
        }



        //subscribers selection
        protected override Task<List<Subscriber<ObjectId>>> LookupStartingWithDeliveryTypes(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var pipeline = new EmptyPipelineDefinition<TDeliveryType>()
                .As<TDeliveryType, TDeliveryType, TDeliveryType>();

            var pipeline2 = pipeline.Match(ToDeliveryTypeSettingsFilter(parameters, subscribersRange))
                .As<TDeliveryType, TDeliveryType, BsonDocument>();
            
            //var bsonDocs = _context.SubscriberDeliveryTypeSettings.Aggregate(pipeline2).ToListAsync();
            //string json = subscribers.Select(x => x.ToJsonIntended()).Join(",");
            //return null;

            PipelineDefinition<TDeliveryType, Subscriber<ObjectId>> pipelineProjected
                = AddSubscribersProjectionAndLimitStage(pipeline2, subscribersRange);

            return _collectionFactory
                .GetCollection<TDeliveryType>()
                .Aggregate(pipelineProjected)
                .ToListAsync();
        }


        //filters
        public override FilterDefinition<TDeliveryType> ToDeliveryTypeSettingsFilter(
            SubscriptionParameters parameters, SubscribersRangeParameters<ObjectId> subscribersRange)
        {
            var filter = base.ToDeliveryTypeSettingsFilter(parameters, subscribersRange);

            if (subscribersRange.SelectFromCategories)
            {
                var categoriesFilter = ToCategorySettingsFilter(parameters, subscribersRange);
                filter &= Builders<TDeliveryType>.Filter
                    .ElemMatch(x => x.SubscriberCategorySettings, categoriesFilter);
            }

            return filter;
        }

    }
}
