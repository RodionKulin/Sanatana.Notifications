using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications.EventsHandling.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.NotificationsTests.EventsHandling.Templates
{
    [TestClass()]
    public class RazorTransformerTests
    {
        [TestMethod()]
        public void RazorTransformer_TransformTest()
        {
            //prepare
            StringTemplate templateProvider = "@Model.Name";
            var templateData = new List<TemplateData>
            {
                new TemplateData(new {
                    Name = "Replaced"
                })
            };

            //invoke
            var target = new RazorTransformer();
            Dictionary<TemplateData, string> filledTemplates = target.Transform(templateProvider, templateData);

            //assert
            Assert.AreEqual(1, filledTemplates.Count);
            Assert.AreEqual("Replaced", filledTemplates.Values.First());
        }
    }
}
