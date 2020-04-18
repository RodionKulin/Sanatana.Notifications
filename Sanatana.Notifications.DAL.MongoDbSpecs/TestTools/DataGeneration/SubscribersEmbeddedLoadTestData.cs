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
using Sanatana.DataGenerator.Internals;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration
{
    public class SubscribersEmbeddedLoadTestData : SubscribersEmbeddedData, IGeneratorData
    {
        //generators
        protected override List<SubscriberTopicSettings<ObjectId>> GenerateTopicsForDeliveryTypeSettings(GeneratorContext genContext,
            SpecsDeliveryTypeSettings dt, SubscriberWithMissingData subscriber)
        {
            EntityContext subscriberContext = genContext.EntityContexts[typeof(SubscriberWithMissingData)];
            long subscriberNumber = subscriberContext.EntityProgress.CurrentCount;
            int subscribersPerTopic = 1000;
            long subscribersTopicGroup = subscriberNumber / subscribersPerTopic;

            string[] topics = new[]
            {
                "301" + subscribersTopicGroup,
                "302" + subscribersTopicGroup
            };

            var categoryTopics = dt.SubscriberCategorySettings.SelectMany(category =>
                topics.Select((topic, i) => new SubscriberTopicSettings<ObjectId>
                {
                    SubscriberTopicSettingsId = ObjectId.GenerateNewId(),
                    SubscriberId = dt.SubscriberId,
                    DeliveryType = dt.DeliveryType,
                    CategoryId = category.CategoryId,
                    TopicId = topic,
                    IsEnabled = true,
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
