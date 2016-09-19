using Common.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.DAL.MongoDb;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb.Tests
{
    [TestClass()]
    public class MongoDbUserTopicSettingsQueriesTests
    {
        [TestMethod()]
        public async Task SelectTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new MongoDbUserTopicSettingsQueries(logger, SignaloBotTestParameters.MongoDbConnection);

            //проверка
            var result = await target.Select(SignaloBotTestParameters.ExistingUserObjectId,
                SignaloBotTestParameters.ExistingCategoryID
                , SignaloBotTestParameters.ExistingSubscriptionTopicID);
            Assert.IsFalse(result.HasExceptions);            
        }
    }
}