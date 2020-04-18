using MongoDB.Driver;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.MongoDb;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.Notifications.DAL.Entities;
using MongoDB.Bson;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration
{
    public class SubscribersData : IGeneratorData
    {
        //methods
        public virtual void RegisterEntities(GeneratorSetup setup, SpecsDbContext dbContext)
        {
            IMongoDatabase _database = dbContext.SignalBounces.Database;

            setup.RegisterEntity<SubscriberWithMissingData>()
                .SetGenerator(GenerateSubscriber);

            setup.RegisterEntity<SpecsDeliveryTypeSettings>()
                .SetMultiGenerator<SubscriberWithMissingData>(GenerateDeliveryTypes)
                .SetPersistentStorage(new MongoDbPersistentStorage(_database, dbContext.SubscriberDeliveryTypeSettings.CollectionNamespace.CollectionName));

            setup.RegisterEntity<SubscriberCategorySettings<ObjectId>>()
                .SetMultiGenerator<SpecsDeliveryTypeSettings, SubscriberWithMissingData>(GenerateCategories)
                .SetPersistentStorage(new MongoDbPersistentStorage(_database, dbContext.SubscriberCategorySettings.CollectionNamespace.CollectionName));

            setup.RegisterEntity<SubscriberTopicSettings<ObjectId>>()
                .SetMultiGenerator<SubscriberCategorySettings<ObjectId>, SubscriberWithMissingData>(GenerateTopics)
                .SetPersistentStorage(new MongoDbPersistentStorage(_database, dbContext.SubscriberTopicSettings.CollectionNamespace.CollectionName));
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
