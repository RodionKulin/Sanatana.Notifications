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
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DeliveryTypes.Email;
using Should.Core;
using SpecsFor.StructureMap;
using SpecsFor.Core;
using Moq;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.Queries
{
    public class SqlSubscriberQueriesSpecs
    {
        [TestFixture]
        public class when_matching_subscribers_by_id_using_ef
           : SpecsFor<SqlSubscriberQueries>, INeedDbContext
        {
            private int _deliveryType = 10;
            private int _categoryId = 10;
            private string _topicId = "10";
            private List<long> _subscriberIds;
            private List<Subscriber<long>> _matchedSubscribers;
            public SenderDbContext DbContext { get; set; }

            protected override void Given()
            {
                _subscriberIds = new List<long>
                {
                    15, 16, 17
                };

                List<SubscriberDeliveryTypeSettingsLong> deliveryTypeSettings = _subscriberIds
                    .Select(x => new SubscriberDeliveryTypeSettingsLong
                    {
                        SubscriberId = x,
                        DeliveryType = _deliveryType,
                        Address = $"subscriber{x}@mail.mail",
                        IsEnabled = true,
                        Language = "en",
                        TimeZoneId = "+3",
                    })
                    .ToList();
                DbContext.SubscriberDeliveryTypeSettings.AddRange(deliveryTypeSettings);

                List<SubscriberCategorySettingsLong> categorySettings = _subscriberIds
                    .Select(x => new SubscriberCategorySettingsLong
                    {
                        SubscriberId = x,
                        DeliveryType = _deliveryType,
                        CategoryId = _categoryId,
                        IsEnabled = true
                    })
                    .ToList();
                DbContext.SubscriberCategorySettings.AddRange(categorySettings);

                List<SubscriberTopicSettingsLong> topicSettings = _subscriberIds
                    .Select(x => new SubscriberTopicSettingsLong
                    {
                        SubscriberId = x,
                        DeliveryType = _deliveryType,
                        CategoryId = _categoryId,
                        TopicId = _topicId,
                        IsEnabled = true
                    })
                    .ToList();
                DbContext.SubscriberTopicSettings.AddRange(topicSettings);

                DbContext.SaveChanges();
            }
            
            protected override void When()
            {
                var subscriberParameters = new SubscriptionParameters()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    TopicId = _topicId,
                    CheckIsNDRBlocked = true,
                    CheckCategoryEnabled = true,
                    CheckTopicEnabled = true,
                    CheckDeliveryTypeEnabled = true
                };

                var subscribersRange = new SubscribersRangeParameters<long>()
                {
                    FromSubscriberIds = _subscriberIds
                };

                _matchedSubscribers = SUT.Select(subscriberParameters, subscribersRange).Result;
            }

            [Test]
            public void then_subscribers_matched_by_id_are_found_using_ef()
            {
                _matchedSubscribers.Should().NotBeEmpty();
                _matchedSubscribers.Count.Should().Be(_subscriberIds.Count);

                _matchedSubscribers = _matchedSubscribers.OrderBy(x => x.SubscriberId).ToList();
                _subscriberIds = _subscriberIds.OrderBy(x => x).ToList();

                for (int i = 0; i < _subscriberIds.Count; i++)
                {
                    Subscriber<long> actualItem = _matchedSubscribers[i];
                    long expectedId = _subscriberIds[i];
                    actualItem.DeliveryType.Should().Be(_deliveryType);
                    actualItem.Address.Should().NotBeEmpty();
                }
            }
        }


        [TestFixture]
        public class when_updating_subscribers_by_id_using_ef
           : SpecsFor<SqlSubscriberQueries>, INeedDbContext
        {
            private int _deliveryType = 10;
            private int _categoryId = 11;
            private string _topicId = "12";
            private List<long> _subscriberIds = new List<long> { 20, 21, 22  };
            private DateTime _sendDate;
            public SenderDbContext DbContext { get; set; }


            public SignalDispatch<long> CreateDispatch(long subscriberId)
            {
                return new EmailDispatch<long>()
                {
                    DeliveryType = _deliveryType,
                    CategoryId = _categoryId,
                    TopicId = _topicId,

                    ReceiverSubscriberId = subscriberId,
                    ReceiverAddress = "fake@mail.mail",
                    ReceiverDisplayName = "Receiver display name",

                    MessageSubject = "subject text",
                    MessageBody = "body text",
                    IsBodyHtml = false,

                    CreateDateUtc = DateTime.UtcNow,
                    SendDateUtc = DateTime.UtcNow,
                    IsScheduled = false,
                    FailedAttempts = 10
                };
            }

            protected override void Given()
            {
                List<SubscriberDeliveryTypeSettingsLong> deliveryTypeSettings = _subscriberIds
                    .Select(x => new SubscriberDeliveryTypeSettingsLong
                    {
                        SubscriberId = x,
                        DeliveryType = _deliveryType,
                        Address = $"subscriber{x}@mail.mail",
                        IsEnabled = true,
                        Language = "en",
                        TimeZoneId = "+3",
                    })
                    .ToList();
                DbContext.SubscriberDeliveryTypeSettings.AddRange(deliveryTypeSettings);

                List<SubscriberCategorySettingsLong> categorySettings = _subscriberIds
                    .Select(x => new SubscriberCategorySettingsLong
                    {
                        SubscriberId = x,
                        DeliveryType = _deliveryType,
                        CategoryId = _categoryId,
                        IsEnabled = true
                    })
                    .ToList();
                DbContext.SubscriberCategorySettings.AddRange(categorySettings);

                List<SubscriberTopicSettingsLong> topicSettings = _subscriberIds
                    .Select(x => new SubscriberTopicSettingsLong
                    {
                        SubscriberId = x,
                        DeliveryType = _deliveryType,
                        CategoryId = _categoryId,
                        TopicId = _topicId,
                        IsEnabled = true
                    })
                    .ToList();
                DbContext.SubscriberTopicSettings.AddRange(topicSettings);

                DbContext.SaveChanges();
            }

            protected override void When()
            {
                var updateParameters = new UpdateParameters()
                {
                    UpdateDeliveryTypeLastSendDateUtc = true,
                    UpdateDeliveryTypeSendCount = true,

                    UpdateCategorySendCount = true,
                    UpdateCategoryLastSendDateUtc = true,
                    CreateCategoryIfNotExist = true,

                    UpdateTopicSendCount = true,
                    UpdateTopicLastSendDateUtc = true,
                    CreateTopicIfNotExist = true
                };

                _sendDate = DateTime.UtcNow;
                List<SignalDispatch<long>> dispatches = _subscriberIds
                    .Select(x => CreateDispatch(x))
                    .ToList();

                SUT.Update(updateParameters, dispatches).Wait();
            }

            [Test]
            public void then_delivery_settings_matched_by_subscriber_id_are_updated_using_ef()
            {
                SenderDbContext dbContext = DbContext;
                
                List<SubscriberDeliveryTypeSettingsLong> _matchedSubscribers = dbContext.SubscriberDeliveryTypeSettings
                    .Where(x => _subscriberIds.Contains(x.SubscriberId))
                    .ToList();

                _matchedSubscribers.Should().NotBeEmpty();
                _matchedSubscribers.Count.Should().Be(_subscriberIds.Count);

                _matchedSubscribers = _matchedSubscribers.OrderBy(x => x.SubscriberId).ToList();
                _subscriberIds = _subscriberIds.OrderBy(x => x).ToList();

                for (int i = 0; i < _subscriberIds.Count; i++)
                {
                    SubscriberDeliveryTypeSettingsLong actualItem = _matchedSubscribers[i];
                    actualItem.DeliveryType.Should().Be(_deliveryType);
                    actualItem.Address.Should().NotBeEmpty();
                    actualItem.SendCount.Should().Be(1);
                    actualItem.LastSendDateUtc.Should().NotBeNull();
                    actualItem.LastSendDateUtc.Value.Should().BeCloseTo(_sendDate, 1000);
                }
            }
            
            [Test]
            public void then_category_settings_matched_by_subscriber_id_are_updated_using_ef()
            {
                SenderDbContext dbContext = DbContext;

                List<SubscriberCategorySettingsLong> _matchedSubscribers = dbContext.SubscriberCategorySettings
                    .Where(x => _subscriberIds.Contains(x.SubscriberId))
                    .ToList();

                _matchedSubscribers.Should().NotBeEmpty();
                _matchedSubscribers.Count.Should().Be(_subscriberIds.Count);

                _matchedSubscribers = _matchedSubscribers.OrderBy(x => x.SubscriberId).ToList();
                _subscriberIds = _subscriberIds.OrderBy(x => x).ToList();

                for (int i = 0; i < _subscriberIds.Count; i++)
                {
                    SubscriberCategorySettingsLong actualItem = _matchedSubscribers[i];
                    actualItem.DeliveryType.Should().Be(_deliveryType);
                    actualItem.CategoryId.Should().Be(_categoryId);
                    actualItem.SendCount.Should().Be(1);
                    actualItem.LastSendDateUtc.Should().NotBeNull();
                    actualItem.LastSendDateUtc.Value.Should().BeCloseTo(_sendDate, 1000);
                }
            }

            [Test]
            public void then_topic_settings_matched_by_subscriber_id_are_updated_using_ef()
            {
                SenderDbContext dbContext = DbContext;

                List<SubscriberTopicSettingsLong> _matchedSubscribers = dbContext.SubscriberTopicSettings
                    .Where(x => _subscriberIds.Contains(x.SubscriberId))
                    .ToList();

                _matchedSubscribers.Should().NotBeEmpty();
                _matchedSubscribers.Count.Should().Be(_subscriberIds.Count);

                _matchedSubscribers = _matchedSubscribers.OrderBy(x => x.SubscriberId).ToList();
                _subscriberIds = _subscriberIds.OrderBy(x => x).ToList();

                for (int i = 0; i < _subscriberIds.Count; i++)
                {
                    SubscriberTopicSettingsLong actualItem = _matchedSubscribers[i];
                    actualItem.DeliveryType.Should().Be(_deliveryType);
                    actualItem.CategoryId.Should().Be(_categoryId);
                    actualItem.TopicId.Should().Be(_topicId);
                    actualItem.SendCount.Should().Be(1);
                    actualItem.LastSendDateUtc.Should().NotBeNull();
                    actualItem.LastSendDateUtc.Value.Should().BeCloseTo(_sendDate, 1000);
                }
            }
        }

    }
}
