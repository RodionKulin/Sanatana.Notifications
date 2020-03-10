using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling.DeliveryTypes.Trace;
using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling.Console.Tests
{
    [TestClass()]
    public class TraceDispatcherTests
    {
        [TestMethod()]
        public void TraceDispatcher_SendTest()
        {
            var target = new TraceDispatcher<long>();
            var item = new SignalDispatch<long>
            {
                DeliveryType = 1,
                CategoryId = 1,
                CreateDateUtc = DateTime.UtcNow,
                SendDateUtc = DateTime.UtcNow,
            };

            ProcessingResult result = target.Send(item).Result;
            Assert.AreEqual(ProcessingResult.Success, result);
        }
    }
}