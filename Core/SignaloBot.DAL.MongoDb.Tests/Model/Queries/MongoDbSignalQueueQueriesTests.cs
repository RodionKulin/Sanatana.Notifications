using Common.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using SignaloBot.DAL.MongoDb;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb.Tests
{
    [TestClass()]
    public class MongoDbSignalQueueQueriesTests
    {
        [TestMethod()]
        public void InsertTest()
        {
            //параметры
            var logger = new ShoutExceptionLogger();
            var target = new MongoDbSignalDispatchQueries(logger, SignaloBotTestParameters.MongoDbConnection);

            var items = new List<SignalDispatchBase<ObjectId>>()
            {
                SignaloBotEntityCreator<ObjectId>.CreateSignal(),
                SignaloBotEntityCreator<ObjectId>.CreateSignal()
            };

            //проверка
            bool result = target.Insert(items).Result;
            Assert.IsTrue(result);
        }
    }
}