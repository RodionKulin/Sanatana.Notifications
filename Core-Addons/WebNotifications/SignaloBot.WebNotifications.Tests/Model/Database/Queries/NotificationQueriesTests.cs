using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Utility;
using SignaloBot.DAL.Queries;
using SignaloBot.DAL.Queries.Client;
using SignaloBot.WebNotifications.Tests.Model;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.WebNotifications.Database.Queries;
using SignaloBot.WebNotifications.Entities;

namespace SignaloBot.DAL.Queries.Tests
{
    [TestClass()]
    public class NotificationQueriesTests
    {
        [TestMethod()]
        public void NotificationQueries_InsertTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            NotificationQueries target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);

            Notification notify = NotificationsTestCommonCreator.CreateNotification();

            List<NotificationMeta> notifyMetas = new List<NotificationMeta>()
            {
                NotificationsTestCommonCreator.CreateNotificationMeta(notify.NotificationID),
                NotificationsTestCommonCreator.CreateNotificationMeta(notify.NotificationID, "AuthorName"),
            };

            List<Guid> userIDs = new List<Guid>()
            {
                SignaloBotTestParameters.ExistingUserID,
                SequentialGuid.NewGuid(),
                SequentialGuid.NewGuid(),
                SequentialGuid.NewGuid()
            };

            //проверка
            target.Insert(notify, notifyMetas, userIDs, out exception);
        }

        [TestMethod()]
        public void NotificationQueries_UpsertTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);

            Notification notify = NotificationsTestCommonCreator.CreateNotification();

            List<NotificationMeta> notifyMetas = new List<NotificationMeta>()
            {
                NotificationsTestCommonCreator.CreateNotificationMeta(notify.NotificationID),
                NotificationsTestCommonCreator.CreateNotificationMeta(notify.NotificationID, "AuthorName"),
            };

            List<Guid> userIDs = new List<Guid>()
            {
                SignaloBotTestParameters.ExistingUserID,
                SequentialGuid.NewGuid(),
                SequentialGuid.NewGuid(),
                SequentialGuid.NewGuid()
            };

            //проверка
            target.Upsert(notify, notifyMetas, userIDs, out exception);
        }

        [TestMethod()]
        public void NotificationQueries_UpdateDirtyTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            NotificationQueries target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);

            int total;
            List<Notification> notifies = target.SelectUpdateLastVisit(SignaloBotTestParameters.ExistingUserID
                , false, out total, 0, 10, out exception);

            //проверка
            target.UpdateDirty(notifies, out exception);
        }

        [TestMethod()]
        public void NotificationQueries_SelectUpdateLastVisitTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            NotificationQueries target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);
            int firstIndex = 1;
            int lastIndex = 10;
            bool updateLastVisit = true;
            int total;

            //проверка
            List<Notification> notifies = target.SelectUpdateLastVisit(SignaloBotTestParameters.ExistingUserID
                , updateLastVisit, out total, firstIndex, lastIndex, out exception);
        }

        [TestMethod()]
        public void NotificationQueries_DeleteCategoryTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            NotificationQueries target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);
           
            //проверка
            target.DeleteCategory(SignaloBotTestParameters.ExistingUserID
                , SignaloBotTestParameters.ExistingCategoryID, out exception);
        }

        [TestMethod()]
        public void NotificationQueries_DeleteTopicTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            NotificationQueries target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);

            //проверка
            target.DeleteTopic(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingCategoryID
                , SignaloBotTestParameters.ExistingSubscriptionTopicID, out exception);
        }

        [TestMethod()]
        public void NotificationQueries_DeleteTagTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            NotificationQueries target = new NotificationQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);

            //проверка
            target.DeleteTag(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingCategoryID
                , "tag", out exception);
        }
    }
}
