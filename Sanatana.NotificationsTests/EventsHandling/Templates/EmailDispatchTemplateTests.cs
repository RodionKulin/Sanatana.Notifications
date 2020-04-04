using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.NotificationsTests.Resource;

namespace Sanatana.Notifications.EventsHandling.Tests
{
    [TestClass()]
    public class EmailDispatchTemplateTests
    {
        [TestMethod()]
        public void EmailDispatchTemplate_ProvideContentTest()
        {
            //prepare
            var emailTemplate = new EmailDispatchTemplate<long>()
            {
                BodyProvider = new ResourceTemplate(typeof(ContentRes), "ContentKey"),
                BodyTransformer = new ReplaceTransformer()
            };

            var replaceModel = new Dictionary<string, string>()
            {
                { "key", "value" }
            };
            var settings = new EventSettings<long>();
            var signalEvent = new SignalEvent<long>()
            {
                TemplateData = replaceModel
            };
            var subscriber = new Subscriber<long>() { SubscriberId = 2 };
            var subscriberList = new List<Subscriber<long>>() { subscriber };
            var templateData = new List<TemplateData>()
            {
                new TemplateData(replaceModel)
            };

            //invoke
            List<SignalDispatch<long>> actual = emailTemplate.Build(settings, signalEvent, subscriberList, templateData);

            //assert
            EmailDispatch<long> item = (EmailDispatch<long>)actual.First();
            Assert.AreEqual(ContentRes.ContentKey, item.MessageBody);
        }
    }
}
