using SignaloBot.DAL;
using SignaloBot.DAL.Queries;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.Stubs;
using Common.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.SQL;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class SubscriberQueriesTests
    {
        [TestMethod()]
        public void SubscriberQueries_SelectTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            SqlSubscriberQueries target = new SqlSubscriberQueries(logger, SignaloBotTestParameters.SqlConnetion);

            var subscriberParameters = new SubscribtionParameters()
            {
                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                TopicID = SignaloBotTestParameters.ExistingSubscriptionTopicID,
                CheckBlockedOfNDR = true,
                CheckCategoryEnabled = true
            };

            var usersRange = new UsersRangeParameters<Guid>()
            {
                FromUserIDs = new List<Guid>()
                {
                    SignaloBotTestParameters.ExistingUserID,
                    Guid.NewGuid()
                }
            };

            //проверка
            QueryResult<List<Subscriber<Guid>>> subscribers = 
                target.Select(subscriberParameters, usersRange).Result;
        }       

    }
}
