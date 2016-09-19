using SignaloBot.TestParameters.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using SignaloBot.DAL.SQL;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class UserDeliveryTypeSettingsQueriesTests
    {
        [TestMethod()]
        public void UserDeliveryTypeSettings_UpdateNDRSettingsTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);
            var settings = new List<UserDeliveryTypeSettings<Guid>>()
            {
                SignaloBotEntityCreator<Guid>.CreateUserDeliveryTypeSettings(SignaloBotTestParameters.ExistingUserID),
                SignaloBotEntityCreator<Guid>.CreateUserDeliveryTypeSettings(Guid.NewGuid()),
                SignaloBotEntityCreator<Guid>.CreateUserDeliveryTypeSettings(Guid.NewGuid())
            };

            //проверка
            bool result = target.UpdateNDRSettings(settings).Result;
        }

        [TestMethod()]
        public void UserDeliveryTypeSettings_DeleteAllTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            bool result = target.Delete(SignaloBotTestParameters.ExistingUserID).Result;
        }
                
        [TestMethod()]
        public void UserDeliveryTypeSettings_UpdateLastVisitTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            bool result = target.UpdateLastVisit(SignaloBotTestParameters.ExistingUserID).Result;
        }
                      
        [TestMethod()]
        public void UserDeliveryTypeSettings_UpdateTimeZoneTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

            //проверка
            bool result = target.UpdateTimeZone(SignaloBotTestParameters.ExistingUserID, timeZone).Result;
        }
        
        [TestMethod()]
        public void UserDeliveryTypeSettings_DisableAllDeliveryTypesTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            bool result = target.DisableAllDeliveryTypes(SignaloBotTestParameters.ExistingUserID).Result;
        }

        [TestMethod()]
        public void UserDeliveryTypeSettings_CheckAddressExistsTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            QueryResult<bool> exists =
                target.CheckAddressExists(SignaloBotTestParameters.ExistingDeliveryType, "mail@test.ml").Result;
        }

        [TestMethod()]
        public void UserDeliveryTypeSettings_SelectPageTest()
        {  
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlUserDeliveryTypeSettingsQueries(logger, SignaloBotTestParameters.SqlConnetion);

            //проверка
            TotalResult<List<UserDeliveryTypeSettings<Guid>>> list = target.SelectPage(1, 10).Result;
        }
    }
}
