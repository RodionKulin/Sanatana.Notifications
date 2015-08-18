using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.DAL.Entities;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.TestParameters.Model;
using SignaloBot.DAL.Queries.Client;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserCategorySettingsQueriesTests
    {
        [TestMethod()]
        public void UserCategorySettingsQueries_UpsertIsEnabledTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserCategorySettingsQueries(
                SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);
            UserCategorySettings userCategorySettings = SignaloBotEntityCreator.CreateUserCategorySettings();
                
            //проверка
            target.UpsertIsEnabled(userCategorySettings, out exception);
        }

        [TestMethod()]
        public void UserCategorySettingsQueries_DeleteAllTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserCategorySettingsQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);
           
            //проверка
            target.DeleteAll(SignaloBotTestParameters.ExistingUserID, out exception);
        }

    }
}
