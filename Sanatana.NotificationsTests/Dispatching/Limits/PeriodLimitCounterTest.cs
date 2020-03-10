using Sanatana.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Sanatana.Notifications.DispatchHandling.Limits;
using Sanatana.Notifications.DispatchHandling.Limits.JournalStorage;
using Shouldly;

namespace Sanatana.Notifications.DispatchHandling.LimitsTests
{

    [TestClass()]
    public class PeriodLimitCounterTest
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
            var target = new PeriodLimitCounter(limitedPeriods, journalStorage);

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
            PeriodLimitCounter target = new PeriodLimitCounter(limitedPeriods, journalStorage);

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
            var target = new PeriodLimitCounter(limitedPeriods, journalStorage);

            //test 
            for (int i = 0; i < limit; i++)
            {
                target.InsertTime();
            }

            DateTime? actualLimitsEndTime = target.GetLimitsEndTimeUtc();
            Assert.IsNotNull(actualLimitsEndTime);

            TimeSpan actualSleepTime = actualLimitsEndTime.Value - DateTime.UtcNow;
            actualSleepTime.ShouldBe(expectedSleepTime, TimeSpan.FromMilliseconds(100));
        }
    }
}
