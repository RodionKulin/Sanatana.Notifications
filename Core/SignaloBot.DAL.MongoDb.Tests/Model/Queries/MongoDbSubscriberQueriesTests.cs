using Common.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using SignaloBot.DAL.MongoDb;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb.Tests
{
    [TestClass()]
    public class MongoDbSubscriberQueriesTests
    {
        [TestMethod()]
        public void TestGetRangesTest()
        {
            var logger = new ShoutExceptionLogger();
            var target = new MongoDbSubscriberQueries(logger, SignaloBotTestParameters.MongoDbConnection);

            Stopwatch sw = Stopwatch.StartNew();
            long count = target.TestCount().Result;
            TimeSpan countTime = sw.Elapsed;

            sw.Restart();
            int fragments = 6;
            List<ObjectId> rangeBorders = target.TestGetRanges(count, fragments).Result;
            TimeSpan rangeTime = sw.Elapsed;

            Assert.IsTrue(rangeBorders.Count >= (fragments - 1));

            //List<ObjectId> rangeBorders = target.TestGetRanges(100, 10).Result;
        }
    }
}