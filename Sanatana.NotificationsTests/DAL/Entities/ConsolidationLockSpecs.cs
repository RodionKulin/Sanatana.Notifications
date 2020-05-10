using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications.DAL.Entities;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.NotificationsTests.DAL.Entities
{
    [TestClass]
    public class ConsolidationLockSpecs
    {
        [TestMethod]
        public void WhenEqualIsUsedForNotMatchingLocks_Returnstrue()
        {
            //prepare
            var item1 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 1
            };
            var item2 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 1
            };

            //act
            bool areEqual = item1.Equals(item2);

            //assert
            areEqual.ShouldBeTrue();
        }


        [TestMethod]
        public void WhenEqualSignIsUsedForMatchingLocks_ReturnsTrue()
        {
            //prepare
            var item1 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 1
            };
            var item2 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 1
            };

            //act
            bool areEqual = item1 == item2;

            //assert
            areEqual.ShouldBeTrue();
        }


        [TestMethod]
        public void WhenEqualIsUsedForNotMatchingLocks_ReturnsFalse()
        {
            //prepare
            var item1 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 1
            };
            var item2 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 2
            };

            //act
            bool areEqual = item1.Equals(item2);

            //assert
            areEqual.ShouldBeFalse();
        }


        [TestMethod]
        public void WhenEqualSignIsUsedForNotMatchingLocks_ReturnsFalse()
        {
            //prepare
            var item1 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 1
            };
            var item2 = new ConsolidationLock<long>
            {
                CategoryId = 1,
                DeliveryType = 1,
                ReceiverSubscriberId = 2
            };

            //act
            bool areEqual = item1 == item2;

            //assert
            areEqual.ShouldBeFalse();
        }
    }
}
