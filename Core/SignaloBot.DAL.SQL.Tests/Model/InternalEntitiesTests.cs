using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.DAL.SQL;
using Common.TestUtility;

namespace SignaloBot.DAL.Tests
{
    [TestClass()]
    public class InternalEntitiesTests
    {
        [TestMethod()]
        public void UserTopicSettingsTotalTest()
        {
            ReflectionTests.CompareProperties(typeof(UserTopicSettings<Guid>)
                , typeof(UserTopicSettingsTotal)
                , new List<string>() { "TotalRows", "Category" });
        }

        [TestMethod()]
        public void UserDeliveryTypeSettingsTotalTest()
        {
            ReflectionTests.CompareProperties(typeof(UserDeliveryTypeSettings<Guid>)
                , typeof(UserDeliveryTypeSettingsTotal)
                , new List<string>() { "TotalRows" });
        }


        //[TestMethod()]
        //public void UserDeliveryTypeSettingsTest()
        //{
        //    //var settings = new UserDeliveryTypeSettings<Guid>();
        //    //FillObjectUtility.FillObjectMaxLengthContent(settings, 5);

        //    //new UserDeliveryTypeSettingsGuid();

        //    //ReflectionTests.CompareValues(typeof(UserDeliveryTypeSettings<Guid>)
        //    //    , typeof(UserDeliveryTypeSettingsGuid));
        //}
    }
}
