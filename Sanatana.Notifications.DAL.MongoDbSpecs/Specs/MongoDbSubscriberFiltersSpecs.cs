using MongoDB.Bson;
using NUnit.Framework;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.MongoDb.Queries;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using SpecsFor.StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.MongoDb;
using Sanatana.DataGenerator.Storages;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using System.Diagnostics;
using Sanatana.MongoDb.Extensions;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.Specs
{
    class MongoDbSubscriberFiltersSpecs
    {

        //filters specs
        [TestFixture]
        public class when_fitlering_delivery_types
           : SpecsFor<MongoDbSubscriberEmbeddedCategoriesQueries>, INeedDbContext
        {
            private int _deliveryType = 101;
            private int _categoryId = 201;
            private string _topicId = "301a";
            private string _actual_json;

            public SenderMongoDbContext DbContext { get; set; }


            //methods
            protected override void When()
            {
                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    SelectFromTopics = true,
                    TopicId = _topicId,
                    SubscriberIdRangeFromIncludingSelf = new ObjectId("5e62aec745e7c56d244a4de0"),
                    SubscriberIdRangeToIncludingSelf = new ObjectId("5e62aec745e7c56d244a4de1")
                };

                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    TopicId = _topicId,
                    CheckTopicEnabled = true,
                };

                var deliveryTypefilterDefinition = SUT.ToDeliveryTypeSettingsFilter(subscriberParameters, subscribersRange);
                _actual_json = FilterDefinitionExtensions.ToJson(deliveryTypefilterDefinition);
            }

            [Test]
            public void then_json_is_not_empty()
            {
                _actual_json.Should().NotBeEmpty();

                string expected = "{ \"Address\" : { \"$ne\" : null }, \"SubscriberId\" : { \"$gte\" : ObjectId(\"5e62aec745e7c56d244a4de0\"), \"$lte\" : ObjectId(\"5e62aec745e7c56d244a4de1\") }, \"DeliveryType\" : 101 }";
                _actual_json.Should().Match(expected);
            }
        }
    }
}
