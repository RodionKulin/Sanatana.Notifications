using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Serialization;
using TemplatesExample;
using System.Diagnostics;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.NotificationsTests.Resource;

namespace Sanatana.Notifications.EventsHandling.Tests
{
    [TestClass()]
    public class SubjectDispatchTemplateTests
    {
        [TestMethod()]
        public void SubjectDispatchTemplate_ProvideContentTest()
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
        
        //[TestMethod()]
        //public void SubjectDispatchTemplate_ProvideXSLTContentTest()
        //{          
        //    //parameters
        //    var builder = new SubjectDispatchTemplate<long>()
        //    {
        //        BodyProvider = new FileTemplate("Content/ProductsOrder.xslt"),
        //        BodyTransformer =  new XsltTransformer()
        //    };

        //    object objectModel = CreateObjectModel();
        //    Subscriber<long> subscriber = new Subscriber<long>() { UserId = 2 };

        //    //test         
        //    Stopwatch timer = Stopwatch.StartNew();
        //    SubjectDispatch<long> item = builder.Build(subscriberList, dataList);
        //    TimeSpan total = timer.Elapsed;
        //    Assert.IsNotNull(item.MessageBody);
        //}



        //private methods
        private object CreateObjectModel()
        {
            ProductsOrderMailData data = new ProductsOrderMailData();

            Product soap = CreateProduct(1, "melon soap", (decimal)2.3);
            Product shampoo = CreateProduct(2, "shampoo", (decimal)5.5);
            Product towel = CreateProduct(5, "cotton towel", 15);

            data.CustomerName = "Peter";
            data.OrderId = 12321;
            data.OrderDate = DateTime.UtcNow;
            data.Products.Add(soap);
            data.Products.Add(shampoo);
            data.Products.Add(towel);

            return data;
        }        

        private Product CreateProduct(int id, string name, decimal price)
        {
            Product product = new Product();

            product.Id = id;
            product.Name = name;
            product.Price = price;

            return product;
        }
    }
}
