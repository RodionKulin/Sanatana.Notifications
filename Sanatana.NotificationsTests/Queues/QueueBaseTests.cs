using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Queue;
using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.NotificationsTests.TestTools;

namespace Sanatana.Notifications.Queue.Tests
{
    [TestClass()]
    public class QueueBaseTests
    {
        [TestMethod()]
        public void QueueBase_ReturnExtraItemsTest()
        {
            var target = (DispatchQueue<long>)Dependencies.Resolve<IDispatchQueue<long>>();
            target.PersistBeginOnItemsCount = 4;
            target.PersistEndOnItemsCount = 2;

            target.Append(new List<SignalDispatch<long>>()
            {
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch()
            }, true);

            var activeKeys = new List<int>() { 1 };
            target.ReturnExtraItems(activeKeys);

            int totalItems = target.CountQueueItems();
            Assert.AreEqual(target.PersistEndOnItemsCount, totalItems);
        }

        [TestMethod()]
        public void QueueBase_CountTest()
        {
            var target = (DispatchQueue<long>)Dependencies.Resolve<IDispatchQueue<long>>();
            target.Flush();
            target.PersistBeginOnItemsCount = 4;
            target.PersistEndOnItemsCount = 2;

            var itemsList = new List<SignalDispatch<long>>()
            {
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch()
            };
            target.Append(itemsList, true);

            int actualItemsCount = target.CountQueueItems();
            Assert.AreEqual(itemsList.Count, actualItemsCount);
        }


        [TestMethod()]
        public void QueueBase_IsEmptyTest()
        {
            var target = (DispatchQueue<long>)Dependencies.Resolve<IDispatchQueue<long>>();
            target.PersistBeginOnItemsCount = 4;
            target.PersistEndOnItemsCount = 2;

            var itemsList = new List<SignalDispatch<long>>()
            {
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch(),
                CreateDispatch()
            };
            List<int> itemDeliveryTypes = itemsList.Select(x => x.DeliveryType).ToList();
            target.Append(itemsList, true);

            bool actualIsEmpty = target.CheckIsEmpty(itemDeliveryTypes);
            Assert.AreEqual(false, actualIsEmpty);
        }

        public static EmailDispatch<long> CreateDispatch()
        {
            return new EmailDispatch<long>()
            {
                //SignalID = is set in database

                DeliveryType = 1,
                CategoryId = 1,
                TopicId = "1",

                ReceiverSubscriberId = 1,
                ReceiverAddress = "fake@aa.aa",
                ReceiverDisplayName = "receiver display name",

                MessageSubject = "subject text",
                MessageBody = "body text",
                IsBodyHtml = false,

                SendDateUtc = DateTime.UtcNow,
                IsScheduled = false,
                FailedAttempts = 10
            };
        }
    }

}