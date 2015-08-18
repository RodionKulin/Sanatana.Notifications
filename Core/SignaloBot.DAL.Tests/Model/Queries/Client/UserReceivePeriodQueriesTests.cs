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
using Common.Utility;
using SignaloBot.DAL.Queries.Client;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserReceivePeriodQueriesTests
    {
        [TestMethod()]
        public void UserReceivePeriodQueries_RewriteTest()
        {
            //параметры
            Exception exception;
            ICommonLogger logger = new ShoutExceptionLogger();
            var target = new UserReceivePeriodQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

            List<UserReceivePeriod> periods = new List<UserReceivePeriod>()
            {
                SignaloBotEntityCreator.CreateUserReceivePeriod(1, 0),
                SignaloBotEntityCreator.CreateUserReceivePeriod(1, 1),
                SignaloBotEntityCreator.CreateUserReceivePeriod(1, 2)
            };


            //проверка
            target.Rewrite(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingDeliveryType
                , SignaloBotTestParameters.ExistingCategoryID, periods, out exception);                
        }



        [TestMethod()]
        public void UserReceivePeriodQueries_DeleteAllTest()
        {
            //параметры
            Exception exception;
            ICommonLogger logger = new ShoutExceptionLogger();
            var target = new UserReceivePeriodQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);


            //проверка
            target.DeleteAll(SignaloBotTestParameters.ExistingUserID, out exception);
        }

        [TestMethod()]
        public void UserReceivePeriodQueries_DeleteCategoryTest()
        {
            //параметры
            Exception exception;
            ICommonLogger logger = new ShoutExceptionLogger();
            var target = new UserReceivePeriodQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);


            //проверка
            target.DeleteCategory(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingDeliveryType
                , SignaloBotTestParameters.ExistingCategoryID, out exception);
        }
    }
}
