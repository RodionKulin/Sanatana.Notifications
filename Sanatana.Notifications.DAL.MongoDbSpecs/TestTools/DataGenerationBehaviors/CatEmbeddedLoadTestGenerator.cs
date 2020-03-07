using MongoDB.Bson;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGenerationBehaviors
{
    public class CatEmbeddedLoadTestGenerator : CatEmbeddedGenerator
    {
        //data ammount
        protected override void SetDataAmounts(GeneratorSetup setup)
        {
            setup.GetEntityDescription<SubscriberWithMissingData>()
                .SetTargetCount(50000000);
            setup.GetEntityDescription<SubscriberDeliveryTypeSettings<ObjectId>>()
                .SetTargetCount(100000000);
            setup.GetEntityDescription<SubscriberTopicSettings<ObjectId>>()
                .SetTargetCount(400000000);
        }


        //generators
        protected override List<SubscriberTopicSettings<ObjectId>> GenerateTopicsForDeliveryTypeSettings(GeneratorContext genContext,
            SubscriberDeliveryTypeSettings<ObjectId> dt, SubscriberWithMissingData subscriber)
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
