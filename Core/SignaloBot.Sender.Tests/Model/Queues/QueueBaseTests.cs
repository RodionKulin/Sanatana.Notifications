using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.DAL;
using SignaloBot.Sender.Queue;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue.Tests
{
    [TestClass()]
    public class QueueBaseTests
    {
        [TestMethod()]
        public void QueueBase_ReturnExtraItemsTest()
        {
            var target = new DispatchQueue<Guid>(null);
            target.ItemsQueryCount = 2;
            target.ReturnToStorageAfterItemsCount = 4;

            target.Append(new List<SignalDispatchBase<Guid>>()
            {
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal(),
                SignaloBotEntityCreator<Guid>.CreateSignal()
            }, true);

            var activeKeys = new List<int>() { SignaloBotTestParameters.ExistingDeliveryType };
            target.ReturnExtraItems(activeKeys);

            int totalItems = target.CountQueueItems();
            Assert.AreEqual(target.ItemsQueryCount, totalItems);
        }
    }
}