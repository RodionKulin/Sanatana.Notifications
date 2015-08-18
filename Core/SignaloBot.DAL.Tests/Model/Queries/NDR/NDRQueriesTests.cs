using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model;
using SignaloBot.DAL.QueriesNDR;
using SignaloBot.DAL.Entities;
using Common.Utility;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.Database.Tests
{
    [TestClass()]
    public class NDRQueriesTests
    {

        [TestMethod()]
        public void NDRQueries_NDRSettings_UpdateTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new NDRQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);
            var settings = new List<UserDeliveryTypeSettings>()
            {
                SignaloBotEntityCreator.CreateUserDeliveryTypeSettings(SignaloBotTestParameters.ExistingUserID),
                SignaloBotEntityCreator.CreateUserDeliveryTypeSettings(Guid.NewGuid()),
                SignaloBotEntityCreator.CreateUserDeliveryTypeSettings(Guid.NewGuid())
            };

            //проверка
            target.NDRSettings_Update(settings);
        }

    }
}
