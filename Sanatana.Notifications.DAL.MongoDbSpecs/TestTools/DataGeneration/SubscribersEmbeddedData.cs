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
    public class SubscribersEmbeddedData : SubscribersData, IGeneratorData
    {
        //methods
        public override void RegisterEntities(GeneratorSetup setup, SpecsDbContext dbContext)
        {
            IMongoDatabase _database = dbContext.SignalBounces.Database;

            setup.RegisterEntity<SubscriberWithMissingData>()
                .SetGenerator(GenerateSubscriber);

            setup.RegisterEntity<SpecsDeliveryTypeSettings>()
                .SetMultiGenerator<SubscriberWithMissingData>(GenerateDeliveryTypes)
                .SetPersistentStorage(new MongoDbPersistentStorage(_database, dbContext.SubscriberDeliveryTypeSettings.CollectionNamespace.CollectionName));

            setup.RegisterEntity<SubscriberTopicSettings<ObjectId>>()
                .SetMultiGenerator<SubscriberCategorySettings<ObjectId>, SubscriberWithMissingData>(GenerateTopics)
                .SetPersistentStorage(new MongoDbPersistentStorage(_database, dbContext.SubscriberTopicSettings.CollectionNamespace.CollectionName));
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
