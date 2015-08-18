using SignaloBot.DAL;
using SignaloBot.DAL.Enums;
using SignaloBot.DAL.Queries;
using SignaloBot.Database;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.Stubs;
using Common.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Entities.Parameters;
using SignaloBot.DAL.Entities.Results;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class SubscriberQueriesTests
    {
        [TestMethod()]
        public void SubscriberQueries_SelectTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            SubscriberQueries target = new SubscriberQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

            var subscriberParameters = new SubscriberParameters()
            {
                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                TopicID = SignaloBotTestParameters.ExistingSubscriptionTopicID,
                CheckBlockedOfNDR = true,
                CheckCategoryEnabled = true,
                FromUserIDList = new List<Guid>()
                {
                    SignaloBotTestParameters.ExistingUserID,
                    Guid.NewGuid()
                }
            };

            //проверка
            List<Subscriber> subscribers = target.Select(subscriberParameters, out exception);
        }       

    }
}
