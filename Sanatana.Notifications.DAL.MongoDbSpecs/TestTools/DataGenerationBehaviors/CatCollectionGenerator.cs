using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using Sanatana.Notifications.DAL.Entities;
using MongoDB.Bson;
using Sanatana.DataGenerator.Storages;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGenerationBehaviors
{
    public class CatCollectionGenerator : Behavior<INeedSubscriptionsData>
    {
        //fields
        protected bool _isInitialized;
        protected InMemoryStorage _storage;


        //methods
        public override void SpecInit(INeedSubscriptionsData instance)
        {
            if (!_isInitialized)
            {
                _storage = SetupGenerator(instance.Mocker.GetServiceInstance<SpecsDbContext>());
                _isInitialized = true;
            }

            instance.SubscribersGenerated = _storage;
        }

        private InMemoryStorage SetupGenerator(SpecsDbContext dbContext)
        {
            var ammounts = new Dictionary<Type, long>
            {
                [typeof(SubscriberWithMissingData)] = 1000,
                [typeof(SpecsDeliveryTypeSettings)] = 2000,
                [typeof(SubscriberCategorySettings<ObjectId>)] = 4000,
                [typeof(SubscriberTopicSettings<ObjectId>)] = 8000
            };
            return new GeneratorRunner().Generate(
                 dbContext: dbContext,
                 generatorData: new SubscribersData(),
                 useMemoryStorage: true,
                 ammounts: ammounts);
        }
    }
}
