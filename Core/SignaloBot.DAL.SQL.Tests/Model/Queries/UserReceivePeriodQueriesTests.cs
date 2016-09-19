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
using SignaloBot.DAL.Queries.Client;
using SignaloBot.DAL.SQL;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserReceivePeriodQueriesTests
    {
        [TestMethod()]
        public void UserReceivePeriodQueries_RewriteTest()
        {
            //параметры
            ICommonLogger logger = new ShoutExceptionLogger();
            var target = new SqlUserReceivePeriodQueries(logger, SignaloBotTestParameters.SqlConnetion);

            List<UserReceivePeriod<Guid>> periods = new List<UserReceivePeriod<Guid>>()
            {
                SignaloBotEntityCreator<Guid>.CreateUserReceivePeriod(1, 0),
                SignaloBotEntityCreator<Guid>.CreateUserReceivePeriod(1, 1),
                SignaloBotEntityCreator<Guid>.CreateUserReceivePeriod(1, 2)
            };
            
            //проверка
            bool result = target.Rewrite(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingDeliveryType
                , SignaloBotTestParameters.ExistingCategoryID, periods).Result;
        }
        
        [TestMethod()]
        public void UserReceivePeriodQueries_DeleteAllTest()
        {
            //параметры
            ICommonLogger logger = new ShoutExceptionLogger();
            var target = new SqlUserReceivePeriodQueries(logger, SignaloBotTestParameters.SqlConnetion);


            //проверка
            bool result = target.Delete(SignaloBotTestParameters.ExistingUserID).Result;
        }

        [TestMethod()]
        public void UserReceivePeriodQueries_DeleteCategoryTest()
        {
            //параметры
            ICommonLogger logger = new ShoutExceptionLogger();
            var target = new SqlUserReceivePeriodQueries(logger, SignaloBotTestParameters.SqlConnetion);


            //проверка
            bool result = target.Delete(SignaloBotTestParameters.ExistingUserID, SignaloBotTestParameters.ExistingDeliveryType
                , SignaloBotTestParameters.ExistingCategoryID).Result;
        }
    }
}
