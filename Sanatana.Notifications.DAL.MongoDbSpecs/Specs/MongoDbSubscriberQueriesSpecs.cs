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
    public class MongoDbSubscriberQueriesSpecs
    {
        public abstract class base_subscribers_spec : SpecsFor<MongoDbSubscriberQueries>{}

        [TestFixture]
        public class when_matching_subscribers_by_group_id
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private ObjectId _groupId = new ObjectId("5e4041aa2e7e5a38a8ead836");
            private List<Subscriber<ObjectId>> _actualSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CheckDeliveryTypeEnabled = false
                };

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    GroupId = _groupId
                };

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_include_projected_fields()
            {
                _actualSubscribers.ForEach((sub) =>
                {
                    sub.SubscriberId.Should().NotBe(ObjectId.Empty);
                    sub.DeliveryType.Should().Be(_deliveryType);
                    sub.Address.Should().NotBeEmpty();
                });
            }
            [Test]
            public void then_subscribers_count_match_subscribers_with_group_id()
            {
                _actualSubscribers.Should().NotBeEmpty();

                var subscribersWithGroupId = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasGroupId);
                int subscribersWithGroupIdCount = subscribersWithGroupId.Count();
                _actualSubscribers.Count.Should().Be(subscribersWithGroupIdCount);
            }
        }

        [TestFixture]
        public class when_matching_subscribers_by_subscriber_ids
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private List<ObjectId> _subscribersToQuery;
            private List<Subscriber<ObjectId>> _actualSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                _subscribersToQuery = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasAddress)
                    .Take(10)
                    .Select(x => x.SubscriberId)
                    .ToList();

                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType
                };

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    FromSubscriberIds = _subscribersToQuery
                };

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_matched_count_is_expected()
            {
                _actualSubscribers.Should().NotBeEmpty();

                _actualSubscribers.Count.Should().Be(_subscribersToQuery.Count);
            }

            [Test]
            public void then_subscribers_include_projected_fields()
            {
                _actualSubscribers.ForEach((sub) =>
                {
                    sub.SubscriberId.Should().NotBe(ObjectId.Empty);
                    sub.DeliveryType.Should().Be(_deliveryType);
                    sub.Address.Should().NotBeEmpty();
                });
            }
        }

        [TestFixture]
        public class when_matching_subscribers_by_delivery_type
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private List<ObjectId> _subscribersWithEmptyAddressToQuery;
            private List<Subscriber<ObjectId>> _actualSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                _subscribersWithEmptyAddressToQuery = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => !x.HasAddress)
                    .Take(5)
                    .Select(x => x.SubscriberId)
                    .ToList();

                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType
                };

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    FromSubscriberIds = _subscribersWithEmptyAddressToQuery
                };

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_with_empty_address_exist_in_database()
            {
                _subscribersWithEmptyAddressToQuery.Should().NotBeEmpty();
            }

            [Test]
            public void then_subscribers_are_excluded_from_results()
            {
                _actualSubscribers.Should().BeEmpty();
            }
        }

        [TestFixture]
        public class when_matching_topic_lastsenddate_and_lastvisitdate
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private int _categoryId = 201;
            private string _topicId = "301";
            private List<ObjectId> _subscribersWithTopicLastSendDate;
            private List<Subscriber<ObjectId>> _actualSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                _subscribersWithTopicLastSendDate = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasTopicLastSendDate)
                    .Select(x => x.SubscriberId)
                    .ToList();

                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    CheckTopicLastSendDate = true
                };

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    SelectFromTopics = true,
                    TopicId = _topicId,
                    FromSubscriberIds = _subscribersWithTopicLastSendDate
                };

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_database_has_subscribers_with_last_send_date()
            {
                _subscribersWithTopicLastSendDate.Should().NotBeEmpty();
            }

            [Test]
            public void then_subscribers_count_are_matched()
            {
                int expectedMatchedTopics = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasTopicLastSendDate)
                    //has visit date in future or has visit date NULL
                    .Where(x => x.HasVisitDateFuture || !x.HasVisitDatePast)
                    .Count();

                _actualSubscribers.Should().HaveCount(expectedMatchedTopics);
            }
        }

        [TestFixture]
        [Category("LoadTest")]
        public class when_matching_subscribers_by_category_is_enabled
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private int _categoryId = 201;
            private List<Subscriber<ObjectId>> _actualSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    CheckCategoryEnabled = true
                };

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    SelectFromCategories = true,

                };

                int totalMatchingCount = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasAddress && x.HasCategorySettingsEnabled)
                    .Count();

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_matched_count_is_expected()
            {
                Debug.WriteLine("Actual count: " + _actualSubscribers.Count);

                _actualSubscribers.Should().NotBeEmpty();

                int expectedCount = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasAddress && x.HasCategorySettingsEnabled)
                    .Count();
                _actualSubscribers.Count.Should().Be(expectedCount);
            }

            [Test]
            public void then_subscribers_include_projected_fields()
            {
                _actualSubscribers.ForEach((sub) =>
                {
                    sub.SubscriberId.Should().NotBe(ObjectId.Empty);
                    sub.DeliveryType.Should().Be(_deliveryType);
                    sub.Address.Should().NotBeEmpty();
                });
            }

            [Test]
            public void then_measured_duration()
            {
            }
        }

        [TestFixture]
        [Category("LoadTest")]
        public class when_matching_subscribers_by_topic_is_enabled
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private int _categoryId = 201;
            private string _topicId = "301a";
            private List<Subscriber<ObjectId>> _actualSubscribers;
            private List<SubscriberWithMissingData> _expectedSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void Given()
            {
                _expectedSubscribers = GeneratedEntities.GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasAddress && x.HasTopicsSettingsEnabled)
                    .TakeLast(10)
                    .ToList();
            }

            protected override void When()
            {
                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    TopicId = _topicId,
                    CheckTopicEnabled = true,
                };

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    SelectFromTopics = true,
                    TopicId = _topicId
                };

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_matched_count_is_expected()
            {
                Debug.WriteLine("Actual count: " + _actualSubscribers.Count);

                _actualSubscribers.Should().NotBeEmpty();

                int expectedCount = _expectedSubscribers.Count;
                _actualSubscribers.Count.Should().Be(expectedCount);
            }

            [Test]
            public void then_subscribers_include_projected_fields()
            {
                _actualSubscribers.ForEach((sub) =>
                {
                    sub.SubscriberId.Should().NotBe(ObjectId.Empty);
                    sub.DeliveryType.Should().Be(_deliveryType);
                    sub.Address.Should().NotBeEmpty();
                });
            }

            [Test]
            public void then_measured_duration()
            {
            }
        }

        [TestFixture]
        [Category("LoadTest")]
        public class when_matching_subscribers_by_category_is_enabled_ranged
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private int _categoryId = 201;
            private List<Subscriber<ObjectId>> _actualSubscribers;
            private List<SubscriberWithMissingData> _expectedSubscribers;
            private Stopwatch _queryDurationWatch;


            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                _expectedSubscribers = GeneratedEntities
                    .GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasAddress && x.HasCategorySettingsEnabled)
                    .TakeLast(10)
                    .ToList();

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    SelectFromCategories = true,
                    SubscriberIdRangeFromIncludingSelf = _expectedSubscribers.First().SubscriberId,
                    SubscriberIdRangeToIncludingSelf = _expectedSubscribers.Last().SubscriberId
                };

                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    CheckCategoryEnabled = true
                };

                _queryDurationWatch = Stopwatch.StartNew();
                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
                _queryDurationWatch.Stop();
            }

            [Test]
            public void then_subscribers_matched_count_is_expected()
            {
                Debug.WriteLine("Actual count: " + _actualSubscribers.Count);

                _actualSubscribers.Should().NotBeEmpty();

                int expectedCount = _expectedSubscribers.Count();
                _actualSubscribers.Count.Should().Be(expectedCount);
            }

            [Test]
            public void then_measured_duration()
            {
            }
        }

        [TestFixture]
        [Category("LoadTest")]
        public class when_matching_subscribers_by_topic_is_enabled_ranged
           : base_subscribers_spec, INeedDbContext, INeedSubscriptionsData
        {
            private int _deliveryType = 101;
            private int _categoryId = 201;
            private string _topicId = "301a";
            private List<Subscriber<ObjectId>> _actualSubscribers;
            private List<SubscriberWithMissingData> _expectedSubscribers;

            //properties
            public SenderMongoDbContext DbContext { get; set; }
            public InMemoryStorage GeneratedEntities { get; set; }

            protected override void When()
            {
                _expectedSubscribers = GeneratedEntities
                    .GetList<SubscriberWithMissingData>()
                    .Where(x => x.HasAddress && x.HasTopicsSettingsEnabled)
                    .TakeLast(10)
                    .ToList();

                var subscribersRange = new SubscribersRangeParameters<ObjectId>()
                {
                    SelectFromTopics = true,
                    TopicId = _topicId,
                    SubscriberIdRangeFromIncludingSelf = _expectedSubscribers.First().SubscriberId,
                    SubscriberIdRangeToIncludingSelf = _expectedSubscribers.Last().SubscriberId
                };

                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    TopicId = _topicId,
                    CheckTopicEnabled = true,
                };

                _actualSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_matched_count_is_expected()
            {
                Debug.WriteLine($"{GetType().Name} Actual count: " + _actualSubscribers.Count);

                _actualSubscribers.Should().NotBeEmpty();

                int expectedCount = _expectedSubscribers.Count;
                _actualSubscribers.Count.Should().Be(expectedCount);
            }

            [Test]
            public void then_subscribers_include_projected_fields()
            {
                _actualSubscribers.ForEach((sub) =>
                {
                    sub.SubscriberId.Should().NotBe(ObjectId.Empty);
                    sub.DeliveryType.Should().Be(_deliveryType);
                    sub.Address.Should().NotBeEmpty();
                });
            }

            [Test]
            public void then_measured_duration()
            {
            }
        }

    }
}
