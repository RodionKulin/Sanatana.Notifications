using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.TestUtility.Model;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Results;

namespace SignaloBot.DAL.Tests
{
    [TestClass()]
    public class InternalEntitiesTests
    {
        [TestMethod()]
        public void UserTopicSettingsTotalTest()
        {
            ReflectionUtility.CompareProperties(typeof(UserTopicSettings)
                , typeof(UserTopicSettingsTotal)
                , new List<string>() { "TotalRows", "Category" });
        }

        [TestMethod()]
        public void UserDeliveryTypeSettingsTotalTest()
        {
            ReflectionUtility.CompareProperties(typeof(UserDeliveryTypeSettings)
                , typeof(UserDeliveryTypeSettingsTotal)
                , new List<string>() { "TotalRows" });
        }
    }
}
