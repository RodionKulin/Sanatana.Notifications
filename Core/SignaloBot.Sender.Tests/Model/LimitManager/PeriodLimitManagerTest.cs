using SignaloBot.Sender;
using SignaloBot.Sender.Senders.LimitManager;
using SignaloBot.Sender.Senders.LimitManager.JournalStorage;
using Common.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SignaloBot.Sender.Tests
{

    [TestClass()]
    public class PeriodLimitManagerTest
    {
        
        [TestMethod()]
        public void PeriodLimit_GetLimitCapacityTest()
        {
            //setup
            int limit = 5;
            List<LimitedPeriod> limitedPeriods = new List<LimitedPeriod>()
            {
                new LimitedPeriod(TimeSpan.FromMinutes(30), limit)
            };
            IJournalStorage journalStorage = new MemoryJournalStorage();
            var target = new PeriodLimitManager(limitedPeriods, journalStorage);

            //test 
            int expected = limit;
            int actual = target.GetLimitCapacity();            
            Assert.AreEqual(expected, actual);
            
            //test 
            target.InsertTime();
            expected = limit - 1;
            actual = target.GetLimitCapacity();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Тест для CanIncrement
        ///</summary>
        [TestMethod()]
        public void PeriodLimit_CanIncrementTest()
        {
            //setup
            int limit = 5;
            List<LimitedPeriod> limitedPeriods = new List<LimitedPeriod>()
            {
                new LimitedPeriod(TimeSpan.FromMinutes(30), limit)
            };
            IJournalStorage journalStorage = new MemoryJournalStorage();
            PeriodLimitManager target = new PeriodLimitManager(limitedPeriods, journalStorage);

            //test
            bool canIncrement = target.GetLimitCapacity() > 0;
            Assert.AreEqual(true, canIncrement);

            //test 
            for (int i = 0; i < limit; i++)
            {
                target.InsertTime();
            }
            canIncrement = target.GetLimitCapacity() > 0;
            Assert.AreEqual(false, canIncrement);
        }

        [TestMethod()]
        public void PeriodLimit_GetLimitsEndTimeUtcTest()
        {
            //setup
            int limit = 5;
            TimeSpan expectedSleepTime = TimeSpan.FromMinutes(30);
            List<LimitedPeriod> limitedPeriods = new List<LimitedPeriod>()
            {
                new LimitedPeriod(expectedSleepTime, limit)
            };
            IJournalStorage journalStorage = new MemoryJournalStorage();
            var target = new PeriodLimitManager(limitedPeriods, journalStorage);

            //test 
            for (int i = 0; i < limit; i++)
            {
                target.InsertTime();
            }

            DateTime actualLimitsEndTime = target.GetLimitsEndTimeUtc();
            TimeSpan actualSleepTime = actualLimitsEndTime - DateTime.UtcNow;
            TimeSpan actualSleepTimeRounded = TimeUtility.RoundTimeSpan(actualSleepTime);
            Assert.AreEqual(expectedSleepTime, actualSleepTimeRounded);
        }
    }
}
