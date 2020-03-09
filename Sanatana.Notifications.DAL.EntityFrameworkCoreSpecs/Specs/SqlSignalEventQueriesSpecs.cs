using NUnit.Framework;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using SpecsFor.StructureMap;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Queries;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.Queries
{
    public class SqlSignalEventQueriesSpecs
    {
        [TestFixture]
        public class when_signal_event_inserting_using_ef
           : SpecsFor<SqlSignalEventQueries>, INeedDbContext
        {
            private List<SignalEvent<long>> _insertedData;
            private string _topicId = "topic19";

            public SenderDbContext DbContext { get; set; }

            protected override void When()
            {
                _insertedData = new List<SignalEvent<long>>
                {
                    new SignalEvent<long>()
                    {
                        TopicId = _topicId,
                        CategoryId = 1,
                        EventSettingsId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        TemplateData = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 1, 0),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
                    },
                    new SignalEvent<long>()
                    {
                        TopicId = _topicId,
                        CategoryId = 1,
                        EventSettingsId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        TemplateData = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 2, 0),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
            }
                };

                SUT.Insert(_insertedData).Wait();
            }

            [Test]
            public void then_signal_events_inserted_are_found_using_ef()
            {
                List<SignalEventLong> actual = DbContext.SignalEvents
                   .Where(x => x.TopicId == _topicId)
                   .OrderBy(x => x.CreateDateUtc)
                   .ToList();

                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    SignalEventLong actualItem = actual[i];
                    SignalEvent<long> expectedItem = _insertedData[i];

                    expectedItem.SignalEventId = actualItem.SignalEventId;

                    actualItem.Should().BeEquivalentTo(expectedItem);
                }
            }
        }
        
        [TestFixture]
        public class when_signal_event_updating_using_ef
           : SpecsFor<SqlSignalEventQueries>, INeedDbContext
        {
            private List<SignalEventLong> _insertedData;
            private string _topicId = "topic18";
            public SenderDbContext DbContext { get; set; }

            protected override void Given()
            {
                _insertedData = new List<SignalEventLong>
                {
                    new SignalEventLong()
                    {
                        TopicId = _topicId,
                        CategoryId = 1,
                        EventSettingsId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        TemplateData = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 1, 0),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
                    },
                    new SignalEventLong()
                    {
                        TopicId = _topicId,
                        CategoryId = 1,
                        EventSettingsId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        TemplateData = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 2, 0),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
                    }
                };

                DbContext.SignalEvents.AddRange(_insertedData);
                DbContext.SaveChanges();
            }

            protected override void When()
            {
                foreach(SignalEventLong item in _insertedData)
                {
                    item.FailedAttempts = 1;
                    item.EventSettingsId = 2;
                    item.SubscriberIdRangeFrom = 3;
                    item.SubscriberIdRangeTo = 4;
                    item.SubscriberIdFromDeliveryTypesHandled = new List<int> { 6, 7 };
                }

                var list = _insertedData.Cast<SignalEvent<long>>().ToList();
                SUT.UpdateSendResults(list).Wait();
            }

            [Test]
            public void then_signal_events_updated_are_found_using_ef()
            {
                List<SignalEventLong> actual = DbContext.SignalEvents
                   .Where(x => x.TopicId == _topicId)
                   .OrderBy(x => x.CreateDateUtc)
                   .ToList();

                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    SignalEventLong actualItem = actual[i];
                    SignalEvent<long> expectedItem = _insertedData[i];

                    actualItem.Should().BeEquivalentTo(expectedItem);
                }
            }
        }
        
        [TestFixture]
        public class when_signal_event_selecting_using_ef
           : SpecsFor<SqlSignalEventQueries>, INeedDbContext
        {
            private List<SignalEventLong> _insertedData;
            private List<SignalEvent<long>> _actual;
            private string _topicId = "topic17";
            public SenderDbContext DbContext { get; set; }

            protected override void Given()
            {
                _insertedData = new List<SignalEventLong>
                {
                    new SignalEventLong()
                    {
                        TopicId = _topicId,
                        CategoryId = 1,
                        EventSettingsId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        TemplateData = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 1, 0),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
                    },
                    new SignalEventLong()
                    {
                        TopicId = _topicId,
                        CategoryId = 1,
                        EventSettingsId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        TemplateData = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 2, 0),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
                    }
                };

                DbContext.SignalEvents.AddRange(_insertedData);
                DbContext.SaveChanges();
            }

            protected override void When()
            {
                _actual = SUT.Find(10, 2).Result;
            }

            [Test]
            public void then_signal_events_selected_match_inserted_using_ef()
            {
                _actual.Should().NotBeEmpty();
                _actual.Count.Should().BeGreaterOrEqualTo(_insertedData.Count);

                foreach (var actual in _actual)
                {
                    actual.Should().BeEquivalentTo(new
                    {
                        TopicId = "topic1",
                        CategoryId = 1,
                        AddresseeType = AddresseeType.SubscriptionParameters,
                        DataKeyValues = new Dictionary<string, string>
                        {
                            { "1", "1" },
                            { "2", "2" }
                        },
                        PredefinedAddresses = new List<DeliveryAddress>(),
                        PredefinedSubscriberIds = new List<long>(),
                        SubscriberIdFromDeliveryTypesHandled = new List<int>()
                    });
                }
            }
        }
    }
}
