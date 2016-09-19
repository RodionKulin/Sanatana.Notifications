using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.TestParameters.Model;
using Common.Utility;
using SignaloBot.DAL.SQL;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserTopicSettingsQueriesTests
    {
        
        [TestMethod()]
        public void UserTopicSettingsQueries_DeleteTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserTopicSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            bool completed = target.Delete(SignaloBotTestParameters.ExistingUserID).Result;
        }
        
        [TestMethod()]
        public void UserTopicSettingsQueries_SelectPageTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserTopicSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);
            UserTopicSettings<Guid> subscription = SignaloBotEntityCreator<Guid>.CreateUserTopicSettings(SignaloBotTestParameters.ExistingUserID);
            List<int> categoryIDs = new List<int>()
            {
                SignaloBotTestParameters.ExistingCategoryID,
                SignaloBotTestParameters.ExistingCategoryID
            };
            //проверка
            TotalResult<List<UserTopicSettings<Guid>>> subscriptions = target.SelectPage(
                SignaloBotTestParameters.ExistingUserID, categoryIDs, 1, 10).Result;
        }

        [TestMethod()]
        public void UserTopicSettingsQueries_UpsertTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserTopicSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            UserTopicSettings<Guid> subscription = SignaloBotEntityCreator<Guid>.CreateUserTopicSettings(SignaloBotTestParameters.ExistingUserID);
            subscription.UserID = Guid.NewGuid();

            //проверка
            bool completed = target.Upsert(subscription, true).Result;
        }
    }
}
