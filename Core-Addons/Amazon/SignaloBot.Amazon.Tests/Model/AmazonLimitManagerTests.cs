using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.TestParameters.Model;
using SignaloBot.Amazon;
using SignaloBot.Amazon.Sender;
using SignaloBot.Amazon;
using Common.Utility;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.Sender;
using SignaloBot.Sender.Senders.LimitManager;
using SignaloBot.Sender.Senders.LimitManager.JournalStorage;

namespace SignaloBot.Amazon.Tests
{
    [TestClass()]
    public class AmazonLimitManagerTests
    {
        [TestMethod()]
        [Ignore]
        public void GetAmazonQuotaTest()
        {
            //setup
            ICommonLogger logger = new ShoutExceptionLogger();
            AmazonCredentials amazonCredentials = new AmazonCredentials(AmazonTestParameters.AWSRegion
              , AmazonTestParameters.AWSAccessKey, AmazonTestParameters.AWSSecretKey);
            List<LimitedPeriod> limitedPeriods = new List<LimitedPeriod>()
            {
                new LimitedPeriod(TimeSpan.FromMinutes(30), 5)
            };
            IJournalStorage journalStorage = new MemoryJournalStorage();
            AmazonLimitManager target = new AmazonLimitManager(limitedPeriods, journalStorage, amazonCredentials, logger);
            

            //test
            target.GetAmazonQuota();
        }
    }
}
