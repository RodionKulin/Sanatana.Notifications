using SignaloBot.DAL.Queries;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries.Client;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserDeliveryTypeSettingsQueriesTests
    {
        [TestMethod()]
        public void UserDeliveryTypeSettings_DeleteAllTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserDeliveryTypeSettingsQueries(SignaloBotTestParameters.ConnectionString
                , SignaloBotTestParameters.SqlPrefix, logger);

            //проверка
            target.DeleteAll(SignaloBotTestParameters.ExistingUserID, out exception);
        }
                
        [TestMethod()]
        public void UserDeliveryTypeSettings_UpdateLastVisitTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserDeliveryTypeSettingsQueries(SignaloBotTestParameters.ConnectionString
                , SignaloBotTestParameters.SqlPrefix, logger);

            //проверка
            target.UpdateLastVisit(SignaloBotTestParameters.ExistingUserID, out exception);
        }
                      
        [TestMethod()]
        public void UserDeliveryTypeSettings_UpdateTimeZoneTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserDeliveryTypeSettingsQueries(SignaloBotTestParameters.ConnectionString
                , SignaloBotTestParameters.SqlPrefix, logger);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

            //проверка
            target.UpdateTimeZone(SignaloBotTestParameters.ExistingUserID, timeZone, out exception);
        }
        
        [TestMethod()]
        public void UserDeliveryTypeSettings_DisableAllDeliveryTypesTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserDeliveryTypeSettingsQueries(SignaloBotTestParameters.ConnectionString
                , SignaloBotTestParameters.SqlPrefix, logger);

            //проверка
            target.DisableAllDeliveryTypes(SignaloBotTestParameters.ExistingUserID, out exception);
        }

        [TestMethod()]
        public void UserDeliveryTypeSettings_CheckAddressExistsTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserDeliveryTypeSettingsQueries(SignaloBotTestParameters.ConnectionString
                , SignaloBotTestParameters.SqlPrefix, logger);

            //проверка
            bool exists = target.CheckAddressExists("mail@test.ml", SignaloBotTestParameters.ExistingDeliveryType, out exception);
        }

        [TestMethod()]
        public void UserDeliveryTypeSettings_SelectAllDeliveryTypesTest()
        {  //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new UserDeliveryTypeSettingsQueries(SignaloBotTestParameters.ConnectionString
                , SignaloBotTestParameters.SqlPrefix, logger);

            //проверка
            int total;
            List<UserDeliveryTypeSettings> list = target.SelectAllDeliveryTypes(0, 10, out total, out exception);
        }
    }
}
