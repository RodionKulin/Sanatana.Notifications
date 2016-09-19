using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.TestParameters.Model;
using SignaloBot.DAL.Queries.Client;
using Common.Utility;
using SignaloBot.DAL.SQL;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserCategorySettingsQueriesTests
    {
        [TestMethod()]
        public void UserCategorySettingsQueries_UpsertIsEnabledTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserCategorySettingsQueries(
                logger, SignaloBotTestParameters.SqlConnetion);
            UserCategorySettings<Guid> userCategorySettings = 
                SignaloBotEntityCreator<Guid>.CreateUserCategorySettings(SignaloBotTestParameters.ExistingUserID);
                
            //проверка
            bool result = target.UpsertIsEnabled(userCategorySettings).Result;
        }

        [TestMethod()]
        public void UserCategorySettingsQueries_DeleteAllTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserCategorySettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            bool result = target.Delete(SignaloBotTestParameters.ExistingUserID).Result;
        }

    }
}
