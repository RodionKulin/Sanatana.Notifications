using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.TestParameters.Model;
using SignaloBot.WebNotifications.Database.Queries;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class NotificationMetaQueriesTests
    {

        [TestMethod()]
        public void NotificationMetaQueries_InsertNewTypeTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new NotificationMetaQueries(logger
                , SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix);

            string metaType = "AuthorProfile";
            string metaKey = "101";
            string metaValue = "/default?author=101";

            //проверка
            target.InsertNewType(SignaloBotTestParameters.ExistingCategoryID, metaType,
                metaKey, metaValue, out exception);
        }
    }
}
