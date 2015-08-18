using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.TestParameters.Model;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserTopicSettingsQueriesTests
    {
        [TestMethod()]
        public void UserTopicSettingsQueries_UpsertTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserTopicSettingsQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

            //проверка
            target.Upsert(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingDeliveryType
                , SignaloBotTestParameters.ExistingCategoryID
                , SignaloBotTestParameters.ExistingSubscriptionTopicID, true, true, out exception);
        }
        
        [TestMethod()]
        public void UserTopicSettingsQueries_DeleteAllTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserTopicSettingsQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

              //проверка
            target.DeleteAll(SignaloBotTestParameters.ExistingUserID, out exception);
        }
        
        [TestMethod()]
        public void UserTopicSettingsQueries_SelectPageTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserTopicSettingsQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);
            UserTopicSettings subscription = SignaloBotEntityCreator.CreateUserTopicSettings();
            List<int> categoryIDs = new List<int>()
            {
                SignaloBotTestParameters.ExistingCategoryID,
                SignaloBotTestParameters.ExistingCategoryID
            };
            int total;

            //проверка
            List<UserTopicSettings> subscriptions = target.SelectPage(
                SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingDeliveryType
                , categoryIDs, 0, 10, out total, out exception);
        }

        [TestMethod()]
        public void UserTopicSettingsQueries_UpsertIsEnabledTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserTopicSettingsQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

            UserTopicSettings subscription = SignaloBotEntityCreator.CreateUserTopicSettings();
            subscription.UserID = Guid.NewGuid();

            //проверка
            target.UpsertIsEnabled(subscription, out exception);
        }
    }
}
