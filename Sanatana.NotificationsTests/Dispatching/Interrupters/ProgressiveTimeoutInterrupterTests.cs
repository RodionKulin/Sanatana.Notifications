using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications.DispatchHandling.Interrupters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;

namespace Sanatana.Notifications.DispatchHandling.Interrupters.Tests
{
    [TestClass()]
    public class ProgressiveTimeoutInterrupterTests
    {
        [TestMethod()]
        public void ProgressiveTimeoutInterrupter_FailSingleTest()
        {
            var target = new ProgressiveTimeoutInterrupter<long>();
            target.Fail(null, DispatcherAvailability.NotChecked);

            DateTime? timeoutEndTime = target.GetTimeoutEndUtc();
            Assert.AreEqual(null, timeoutEndTime);
        }

        [TestMethod()]
        public void ProgressiveTimeoutInterrupter_FailManyMinTest()
        {
            var target = new ProgressiveTimeoutInterrupter<long>();
            for (int i = 0; i < target.FailedAttemptsCountTimeoutStart; i++)
            {
                target.Fail(null, DispatcherAvailability.NotChecked);
            }

            DateTime? timeoutEndTime = target.GetTimeoutEndUtc();
            DateTime expected = DateTime.UtcNow.Add(target.TimeoutMinDuration);

            timeoutEndTime.Value.ShouldBe(expected, TimeSpan.FromSeconds(1));
        }

        [TestMethod()]
        public void ProgressiveTimeoutInterrupter_FailManyMaxTest()
        {
            var target = new ProgressiveTimeoutInterrupter<long>();
            for (int i = 0; i < 100; i++)
            {
                target.Fail(null, DispatcherAvailability.NotChecked);
            }

            DateTime? timeoutEndTime = target.GetTimeoutEndUtc();
            DateTime expected = DateTime.UtcNow.Add(target.TimeoutMaxDuration);

            timeoutEndTime.Value.ShouldBe(expected, TimeSpan.FromSeconds(1));
        }
    }
}