using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using StructureMap.AutoMocking;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using MongoDB.Bson;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.MongoDb;
using MongoDB.Driver;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Storages;
using System.Diagnostics;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGenerationBehaviors
{
    public class CatEmbeddedGenerator : CatCollectionGenerator
    {
        //register entities
        protected override void RegisterEntities(INeedSubscriptionsData instance, GeneratorSetup setup)
        {
            var dbContext = instance.Mocker.GetServiceInstance<SpecsDbContext>();
            IMongoDatabase db = dbContext.SignalBounces.Database;
            
            setup.RegisterEntity<SubscriberWithMissingData>()
                .SetGenerator(GenerateSubscriber)
                .SetLimitedCapacityFlushTrigger(_flushAfterNumberOfEntities);

            setup.RegisterEntity<SpecsDeliveryTypeSettings>()
                .SetMultiGenerator<SubscriberWithMissingData>(GenerateDeliveryTypes)
                .SetPersistentStorage(new MongoDbPersistentStorage(db, dbContext.SubscriberDeliveryTypeSettings.CollectionNamespace.CollectionName))
                .SetLimitedCapacityFlushTrigger(_flushAfterNumberOfEntities);

            setup.RegisterEntity<SubscriberTopicSettings<ObjectId>>()
                .SetMultiGenerator<SpecsDeliveryTypeSettings, SubscriberWithMissingData>(GenerateTopicsForDeliveryTypeSettings)
                .SetPersistentStorage(new MongoDbPersistentStorage(db, dbContext.SubscriberTopicSettings.CollectionNamespace.CollectionName))
                .SetLimitedCapacityFlushTrigger(_flushAfterNumberOfEntities);
        }

        protected override void SetDataAmounts(GeneratorSetup setup)
        {
            setup.GetEntityDescription<SubscriberWithMissingData>()
                .SetTargetCount(1000);
            setup.GetEntityDescription<SpecsDeliveryTypeSettings>()
                .SetTargetCount(2000);
            setup.GetEntityDescription<SubscriberTopicSettings<ObjectId>>()
                .SetTargetCount(8000);
        }

        protected override void SetMemoryStorage(INeedSubscriptionsData instance, GeneratorSetup setup)
        {
            _storage = new InMemoryStorage();
            instance.GeneratedEntities = _storage;

            setup.GetEntityDescription<SubscriberWithMissingData>()
                .SetPersistentStorage(instance.GeneratedEntities);
            setup.GetEntityDescription<SpecsDeliveryTypeSettings>()
                .SetPersistentStorage(instance.GeneratedEntities);
            setup.GetEntityDescription<SubscriberTopicSettings<ObjectId>>()
                .SetPersistentStorage(instance.GeneratedEntities);
        }


        //generators
        protected override List<SpecsDeliveryTypeSettings> GenerateDeliveryTypes(
            GeneratorContext genContext, SubscriberWithMissingData subscriber)
        {
            List<SpecsDeliveryTypeSettings> deliveryTypes = base.GenerateDeliveryTypes(genContext, subscriber);

            deliveryTypes.ForEach(dt =>
            {
                dt.SubscriberCategorySettings = GenerateCategories(genContext, dt, subscriber);
            });

            return deliveryTypes;
        }

        protected virtual List<SubscriberTopicSettings<ObjectId>> GenerateTopicsForDeliveryTypeSettings(GeneratorContext genContext,
            SpecsDeliveryTypeSettings dt, SubscriberWithMissingData subscriber)
        {
            string[] topics = new[]
            {
                "301a",
                "302a"
            };

            var categoryTopics = dt.SubscriberCategorySettings.SelectMany(category =>
                topics.Select((topic, i) => new SubscriberTopicSettings<ObjectId>
                {
                    SubscriberTopicSettingsId = ObjectId.GenerateNewId(),
                    SubscriberId = dt.SubscriberId,
                    DeliveryType = dt.DeliveryType,
                    CategoryId = category.CategoryId,
                    TopicId = topic,
                    IsEnabled = subscriber.HasTopicsSettingsEnabled,
                    AddDateUtc = DateTime.UtcNow,
                    LastSendDateUtc = subscriber.HasTopicLastSendDate
                        ? (DateTime?)DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5))
                        : null
                })
            ).ToList();

            subscriber.Topics = subscriber.Topics ?? new List<SubscriberTopicSettings<ObjectId>>();
            subscriber.Topics.AddRange(categoryTopics);

            return categoryTopics;
        }
    }
}
