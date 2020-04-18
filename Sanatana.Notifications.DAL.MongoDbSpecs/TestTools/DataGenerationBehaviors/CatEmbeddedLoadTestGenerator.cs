using MongoDB.Bson;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGenerationBehaviors
{
    public class CatEmbeddedLoadTestGenerator : Behavior<INeedSubscriptionsData>
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
                [typeof(SubscriberWithMissingData)] = 5000000,
                [typeof(SpecsDeliveryTypeSettings)] = 10000000,
                [typeof(SubscriberTopicSettings<ObjectId>)] = 40000000
            };
            return new GeneratorRunner().Generate(
                dbContext: dbContext,
                generatorData: new SubscribersEmbeddedData(),
                useMemoryStorage: false,
                ammounts: ammounts);
        }
    }
}
