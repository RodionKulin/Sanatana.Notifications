using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.Database;
using SignaloBot.TestParameters.Model;
using SignaloBot.DAL.Queries;
using SignaloBot.DAL.Entities;
using Common.Utility;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.DAL.Queries.Client;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Parameters;

namespace SignaloBot.DAL.Queries.Client.Tests
{
    [TestClass()]
    public class SignalQueriesTests
    {
        [TestMethod()]
        public void SignalQueries_UpdateCountersTest()
        {
            //параметры
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new SignalQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

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

            var messagesToSend = new List<Signal>()
            {
                SignaloBotEntityCreator.CreateSignal(),
                SignaloBotEntityCreator.CreateSignal()
            };

            //проверка
            target.UpdateCounters(updateParameters, messagesToSend, out exception);
        }

        [TestMethod()]
        public void SignalQueries_InsertTest()
        {
            Exception exception;
            var logger = new ShoutExceptionLogger();
            var target = new SignalQueries(SignaloBotTestParameters.ConnectionString, SignaloBotTestParameters.SqlPrefix, logger);

            var messagesToSend = new List<Signal>()
            {
                SignaloBotEntityCreator.CreateSignal(),
                SignaloBotEntityCreator.CreateSignal()
            };

            //проверка
            target.Insert(messagesToSend, out exception);
        }

    }
}
