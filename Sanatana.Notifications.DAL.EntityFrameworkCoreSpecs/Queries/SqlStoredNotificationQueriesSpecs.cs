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
using Should;
using SpecsFor.ShouldExtensions;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.Queries
{
    public class SqlStoredNotificationQueriesSpecs
    {
        [TestFixture]
        public class when_stored_notification_inserting_using_ef
           : SpecsFor<SqlStoredNotificationQueries>, INeedDbContext
        {
            private List<StoredNotification<long>> _insertedData;
            private long _subscriberId = 5;
            public SenderDbContext DbContext { get; set; }

            protected override void When()
            {
                _insertedData = new List<StoredNotification<long>>
                {
                    new StoredNotification<long>()
                    {
                        TopicId = "topic1",
                        CategoryId = 1,
                        CreateDateUtc = DateTime.UtcNow,
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    },
                    new StoredNotification<long>()
                    {
                        TopicId = "topic1",
                        CategoryId = 2,
                        CreateDateUtc = DateTime.UtcNow.AddSeconds(1),
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    }
                };

                SUT.Insert(_insertedData).Wait();
            }

            [Test]
            public void then_stored_notifications_inserted_are_found_using_ef()
            {
                List<StoredNotificationLong> actual = DbContext.StoredNotifications
                   .Where(x => x.SubscriberId == _subscriberId)
                   .OrderBy(x => x.CreateDateUtc)
                   .ToList();

                actual.ShouldNotBeEmpty();
                actual.Count.ShouldEqual(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    StoredNotificationLong actualItem = actual[i];
                    StoredNotification<long> expectedItem = _insertedData[i];

                    expectedItem.StoredNotificationId = actualItem.StoredNotificationId;

                    actualItem.ShouldLookLike(expectedItem);
                }
            }
        }
        
        [TestFixture]
        public class when_stored_notification_updating_using_ef
           : SpecsFor<SqlStoredNotificationQueries>, INeedDbContext
        {
            private List<StoredNotificationLong> _insertedData;
            private long _subscriberId = 6;
            public SenderDbContext DbContext { get; set; }

            protected override void Given()
            {
                _insertedData = new List<StoredNotificationLong>
                {
                    new StoredNotificationLong()
                    {
                        TopicId = "topic1",
                        CategoryId = 1,
                        CreateDateUtc = DateTime.UtcNow,
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    },
                    new StoredNotificationLong()
                    {
                        TopicId = "topic1",
                        CategoryId = 2,
                        CreateDateUtc = DateTime.UtcNow.AddSeconds(1),
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    }
                };

                DbContext.StoredNotifications.AddRange(_insertedData);
                DbContext.SaveChanges();
            }

            protected override void When()
            {
                foreach(StoredNotificationLong item in _insertedData)
                {
                    item.MessageBody = "Updated 1";
                    item.TopicId = "topic2";
                }

                var list = _insertedData.Cast<StoredNotification<long>>().ToList();
                SUT.Update(list).Wait();
            }

            [Test]
            public void then_stored_notifications_updated_are_found_using_ef()
            {
                List<StoredNotificationLong> actual = DbContext.StoredNotifications
                   .Where(x => x.SubscriberId == _subscriberId)
                   .OrderBy(x => x.CreateDateUtc)
                   .ToList();

                actual.ShouldNotBeEmpty();
                actual.Count.ShouldEqual(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    StoredNotificationLong actualItem = actual[i];
                    StoredNotification<long> expectedItem = _insertedData[i];

                    actualItem.ShouldLookLike(expectedItem);
                }
            }
        }
        
        [TestFixture]
        public class when_stored_notification_selecting_using_ef
           : SpecsFor<SqlStoredNotificationQueries>, INeedDbContext
        {
            private List<StoredNotificationLong> _insertedData;
            private List<StoredNotification<long>> _actual;
            private long _subscriberId = 7;
            public SenderDbContext DbContext { get; set; }

            protected override void Given()
            {
                _insertedData = new List<StoredNotificationLong>
                {
                    new StoredNotificationLong()
                    {
                        TopicId = "topic1",
                        CategoryId = 1,
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 5, 0),
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    },
                    new StoredNotificationLong()
                    {
                        TopicId = "topic1",
                        CategoryId = 2,
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 5, 0),
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    }
                };

                DbContext.StoredNotifications.AddRange(_insertedData);
                DbContext.SaveChanges();
            }

            protected override void When()
            {
                TotalResult<List<StoredNotification<long>>> totalResult = 
                    SUT.Select(new List<long> { _subscriberId }, 1, 10, false).Result;
                _actual = totalResult.Data;
            }

            [Test]
            public void then_stored_notifications_selected_match_inserted_using_ef()
            {
                _actual.ShouldNotBeEmpty();
                _actual.Count.ShouldBeGreaterThanOrEqualTo(_insertedData.Count);

                foreach (var actual in _actual)
                {
                    actual.ShouldLookLikePartial(new
                    {
                        TopicId = "topic1",
                        CreateDateUtc = new DateTime(2000, 1, 2, 3, 4, 5, 0),
                        MessageBody = "body",
                        MessageSubject = "subject",
                        SubscriberId = _subscriberId
                    });
                }
            }
        }
    }
}
