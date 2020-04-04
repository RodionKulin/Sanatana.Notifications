using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Sanatana.Notifications.EventsHandling.Templates;

namespace Sanatana.Notifications.EventsHandling.Tests
{
    [TestClass()]
    public class LimitedLengthReplaceTransformerTests
    {
        [TestMethod()]
        public void LimitedLengthReplaceTransformer_ShortenSubjectStringTest()
        {
            string stringFormat = "{0}. Static part of the {0} item format.";
            string dynamicPart = "Dynamically generated";

            int staticPartLength = string.Format(stringFormat, string.Empty).Length;
            int dynamicStringRepeatTimes = new Regex(@"\{0\}", RegexOptions.IgnoreCase).Matches(stringFormat).Count;
           
            string shortSuffix = "...";
            int expectedFullStringLength = staticPartLength + dynamicPart.Length * dynamicStringRepeatTimes;            
            int shortenedFullStringLength = expectedFullStringLength - shortSuffix.Length * dynamicStringRepeatTimes;

            var target = new LimitedLengthReplaceTransformer();
            string shortenedString = target.ShortenSubjectString(
                dynamicPart, staticPartLength, shortenedFullStringLength, dynamicStringRepeatTimes);

            Assert.IsTrue(shortenedString.EndsWith(shortSuffix));
        }
    }
}
