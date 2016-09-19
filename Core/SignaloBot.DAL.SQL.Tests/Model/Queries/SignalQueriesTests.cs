using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model;
using SignaloBot.DAL.Queries;
using Common.Utility;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.DAL.Queries.Client;
using SignaloBot.DAL.SQL;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class SignalQueriesTests
    {
        [TestMethod()]
        public void SignalQueries_UpdateCountersTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new SqlSignalQueries(logger, SignaloBotTestParameters.SqlConnetion);

            var updateParameters = new UpdateParameters()
            {
                UpdateDeliveryTypeLastSendDateUtc = true,

                UpdateCategorySendCount = true,
                UpdateCategoryLastSendDateUtc = true,
                CreateCategoryIfNotExist = true,

                UpdateTopicSendCount = false,
                UpdateTopicLastSendDateUtc = true,
                CreateTopicIfNotExist = true
            };

            var items = new List<SignalDispatchBase<Guid>>()
            {
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal()
            };

            //проверка
            bool result = target.UpdateCounters(updateParameters, items).Result;
        }

        [TestMethod()]
        public void SignalQueries_InsertTest()
        {
            var logger = new ShoutExceptionLogger();
            var target = new SqlSignalQueries(logger, SignaloBotTestParameters.SqlConnetion);

            var items = new List<SignalDispatchBase<Guid>>()
            {
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal()
            };

            //проверка
            target.Insert(items);
        }

    }
}
