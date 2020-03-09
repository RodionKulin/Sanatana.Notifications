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
    public class CatCollectionGenerator : Behavior<INeedSubscriptionsData>
    {
        //fields
        protected bool _isInitialized;
        protected InMemoryStorage _storage;
        protected long _flushAfterNumberOfEntities = 50000;


        //methods
        public override void SpecInit(INeedSubscriptionsData instance)
        {
            if (_isInitialized)
            {
                instance.GeneratedEntities = _storage;
                return;
            }
            _isInitialized = true;

            var setup = new GeneratorSetup();
            RegisterEntities(instance, setup);
            SetDataAmounts(setup);
            SetMemoryStorage(instance, setup);

            setup.ProgressChanged += PrintProgress;
            setup.Generate();
        }


        //register entities
        protected virtual void RegisterEntities(INeedSubscriptionsData instance, GeneratorSetup setup)
        {
            var dbContext = instance.Mocker.GetServiceInstance<SpecsDbContext>();
            IMongoDatabase db = dbContext.SignalBounces.Database;

            setup.RegisterEntity<SubscriberWithMissingData>()
                .SetGenerator(GenerateSubscriber)
                .SetLimitedCapacityFlushTrigger(_flushAfterNumberOfEntities);

            setup.RegisterEntity<SpecsDeliveryTypeSettings>()
                .SetMultiGenerator<SubscriberWithMissingData>(GenerateDeliveryTypes)
                .SetPersistentStorage(new MongoDbPersistentStorage(db, dbContext.SubscriberDeliveryTypeSettings.CollectionNamespace.CollectionName));

            setup.RegisterEntity<SubscriberCategorySettings<ObjectId>>()
                .SetMultiGenerator<SpecsDeliveryTypeSettings, SubscriberWithMissingData>(GenerateCategories)
                .SetPersistentStorage(new MongoDbPersistentStorage(db, dbContext.SubscriberCategorySettings.CollectionNamespace.CollectionName))
                .SetLimitedCapacityFlushTrigger(_flushAfterNumberOfEntities);

            setup.RegisterEntity<SubscriberTopicSettings<ObjectId>>()
                .SetMultiGenerator<SubscriberCategorySettings<ObjectId>, SubscriberWithMissingData>(GenerateTopics)
                .SetPersistentStorage(new MongoDbPersistentStorage(db, dbContext.SubscriberTopicSettings.CollectionNamespace.CollectionName))
                .SetLimitedCapacityFlushTrigger(_flushAfterNumberOfEntities);
        }

        protected virtual void SetDataAmounts(GeneratorSetup setup)
        {
            setup.GetEntityDescription<SubscriberWithMissingData>()
                .SetTargetCount(1000);
            setup.GetEntityDescription<SpecsDeliveryTypeSettings>()
                .SetTargetCount(2000);
            setup.GetEntityDescription<SubscriberCategorySettings<ObjectId>>()
                .SetTargetCount(4000);
            setup.GetEntityDescription<SubscriberTopicSettings<ObjectId>>()
                .SetTargetCount(8000);
        }

        protected virtual void SetMemoryStorage(INeedSubscriptionsData instance, GeneratorSetup setup)
        {
            _storage = new InMemoryStorage();
            instance.GeneratedEntities = _storage;

            setup.GetEntityDescription<SubscriberWithMissingData>()
                .SetPersistentStorage(instance.GeneratedEntities);
            setup.GetEntityDescription<SpecsDeliveryTypeSettings>()
                .SetPersistentStorage(instance.GeneratedEntities);
            setup.GetEntityDescription<SubscriberCategorySettings<ObjectId>>()
                .SetPersistentStorage(instance.GeneratedEntities);
            setup.GetEntityDescription<SubscriberTopicSettings<ObjectId>>()
                .SetPersistentStorage(instance.GeneratedEntities);
        }


        //progress
        private void PrintProgress(GeneratorSetup setup, decimal percent)
        {
            Debug.WriteLine($"Data generation progress {percent.ToString("F")}%");
        }


        //generators
        protected virtual SubscriberWithMissingData GenerateSubscriber(GeneratorContext genContext)
        {
            return new SubscriberWithMissingData
            {
                SubscriberId = ObjectId.GenerateNewId(),
                HasAddress = genContext.CurrentCount > 0,
                HasDeliveryTypeSettings = genContext.CurrentCount > 3,
                HasCategorySettingsEnabled = genContext.CurrentCount > 9,
                HasTopicsSettingsEnabled = genContext.CurrentCount > 5,
                HasTopicLastSendDate = new List<long> { 11, 12, 13 }.Contains(genContext.CurrentCount),
                HasVisitDateFuture = genContext.CurrentCount == 12,
                HasVisitDatePast = genContext.CurrentCount == 13,
            };
        }

        protected virtual List<SpecsDeliveryTypeSettings> GenerateDeliveryTypes(
            GeneratorContext genContext, SubscriberWithMissingData subscriber)
        {
            int[] deliveryTypes = new[] { 101, 102 };

            subscriber.DeliveryTypes = deliveryTypes
                .Select((deliveryType, i) => new SpecsDeliveryTypeSettings
                {
                    SubscriberDeliveryTypeSettingsId = ObjectId.GenerateNewId(),
                    SubscriberId = subscriber.SubscriberId,
                    DeliveryType = deliveryType,
                    Address = subscriber.HasAddress ? $"subscriber{genContext.CurrentCount + i}@mail.mail" : null,
                    IsEnabled = subscriber.HasDeliveryTypeSettings,
                    Language = "en",
                    TimeZoneId = "+3",
                    LastVisitUtc = subscriber.HasVisitDateFuture ? (DateTime?)DateTime.UtcNow : null
                })
                .ToList();

            if (subscriber.HasVisitDatePast)
            {
                subscriber.DeliveryTypes.ForEach(x => x.LastVisitUtc = DateTime.UtcNow.AddDays(-1));
            }

            return subscriber.DeliveryTypes;
        }

        protected virtual List<SubscriberCategorySettings<ObjectId>> GenerateCategories(GeneratorContext genContext,
            SpecsDeliveryTypeSettings deliverySettings, SubscriberWithMissingData subscriber)
        {
            int[] categories = new[] { 201, 202 };

            var deliveryTypeCategories = categories
                .Select((category, i) => new SubscriberCategorySettings<ObjectId>
                {
                    SubscriberCategorySettingsId = ObjectId.GenerateNewId(),
                    SubscriberId = deliverySettings.SubscriberId,
                    DeliveryType = deliverySettings.DeliveryType,
                    CategoryId = category,
                    IsEnabled = subscriber.HasCategorySettingsEnabled
                })
                .ToList();

            subscriber.Categories = subscriber.Categories ?? new List<SubscriberCategorySettings<ObjectId>>();
            subscriber.Categories.AddRange(deliveryTypeCategories);

            deliverySettings.SubscriberCategorySettings = deliveryTypeCategories;

            return deliveryTypeCategories;
        }

        protected virtual List<SubscriberTopicSettings<ObjectId>> GenerateTopics(GeneratorContext genContext,
            SubscriberCategorySettings<ObjectId> category, SubscriberWithMissingData subscriber)
        {
            string[] topics = new[]
            {
                "301a",
                "302a"
            };

            var categoryTopics = topics
                .Select((topic, i) => new SubscriberTopicSettings<ObjectId>
                {
                    SubscriberTopicSettingsId = ObjectId.GenerateNewId(),
                    SubscriberId = category.SubscriberId,
                    DeliveryType = category.DeliveryType,
                    CategoryId = category.CategoryId,
                    TopicId = topic,
                    IsEnabled = subscriber.HasTopicsSettingsEnabled,
                    AddDateUtc = DateTime.UtcNow,
                    LastSendDateUtc = subscriber.HasTopicLastSendDate
                        ? (DateTime?)DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5))
                        : null
                })
                .ToList();

            subscriber.Topics = subscriber.Topics ?? new List<SubscriberTopicSettings<ObjectId>>();
            subscriber.Topics.AddRange(categoryTopics);

            return categoryTopics;
        }
    }
}
