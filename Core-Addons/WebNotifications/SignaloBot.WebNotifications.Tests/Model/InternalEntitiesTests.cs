using Common.TestUtility.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.WebNotifications.Entities;
using SignaloBot.WebNotifications.Entities.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Tests
{
    [TestClass()]
    public class InternalEntitiesTests
    {
        [TestMethod()]
        public void NotificationTotalTest()
        {
            ReflectionUtility.CompareProperties(typeof(Notification)
                , typeof(NotificationTotal)
                , new List<string>() { "TotalRows" });
        }

    }
}
