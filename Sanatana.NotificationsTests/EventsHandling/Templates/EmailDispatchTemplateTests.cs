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
        [DataRow("en-US", "Eng-ContentValue")]
        [DataRow("ru-RU", "Ru-ContentValue")]
        public void EmailDispatchTemplate_BuildTest(string language, string expectedText)
        {
            //arrange
            var emailTemplate = new EmailDispatchTemplate<long>()
            {
                BodyProvider = new ResourceTemplate(typeof(ContentRes), "ContentKey"),
                BodyTransformer = new ReplaceTransformer()
            };

            var settings = new EventSettings<long>()
            {
                Subscription = new DAL.Parameters.SubscriptionParameters
                {
                    CategoryId = 1
                }
            };
            var signalEvent = new SignalEvent<long>();
            var subscriberList = new List<Subscriber<long>>() 
            {
                new Subscriber<long>() { 
                    SubscriberId = 2,
                    Language = language
                }
            };
            var templateData = new List<TemplateData>()
            {
                new TemplateData(
                    keyValueModel: new Dictionary<string, string>()
                    {
                        { "key", "value" }
                    }, 
                    objectModel: null,
                    language: language
                )
            };

            //act
            List<SignalDispatch<long>> actual = emailTemplate.Build(settings, signalEvent, subscriberList, templateData);

            //assert
            EmailDispatch<long> item = (EmailDispatch<long>)actual.First();
            Assert.AreEqual(expectedText, item.MessageBody);
        }
    }
}
