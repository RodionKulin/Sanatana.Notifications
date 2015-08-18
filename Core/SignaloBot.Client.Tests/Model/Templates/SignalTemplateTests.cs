using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignaloBot.Client.Tests.Resource;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Serialization;
using TemplatesExample;
using System.Diagnostics;
using SignaloBot.Client.Manager;
using SignaloBot.Client.Templates;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.Results;

namespace SignaloBot.Client.Tests
{
    [TestClass()]
    public class SignalTemplateTests
    {
        [TestMethod()]
        public void MessageTemplate_ProvideContentTest()
        {
            //параметры
            var builder = new SignalTemplate()
            {
                BodyProvider = new ResourceTemplate(typeof(ContentRes), "ContentKey"),
                BodyTransformer = new ReplaceTransformer()
            };

            var replaceModel = new Dictionary<string, string>()
            {
                { "key", "value" }
            };
            var subscriber = new Subscriber(){ UserID = Guid.NewGuid() };
            TemplateData bodyData = new TemplateData(replaceModel);

            //проверка
            Signal message = builder.Build(subscriber, bodyData);
            Assert.AreEqual(ContentRes.ContentKey, message.MessageBody);
        }
        
        [TestMethod()]
        public void MessageTemplate_ProvideXSLTContentTest()
        {          
            //параметры
            var builder = new SignalTemplate()
            {
                BodyProvider = new FileTemplate("Content/ProductsOrder.xslt"),
                BodyTransformer =  new XslTransformer()
            };

            object objectModel = CreateObjectModel();
            Subscriber subscriber = new Subscriber() { UserID = Guid.NewGuid() };
            TemplateData bodyData = new TemplateData(objectModel);

            //проверка         
            Stopwatch timer = Stopwatch.StartNew();
            Signal message = builder.Build(subscriber, bodyData);
            TimeSpan total = timer.Elapsed;
            Assert.IsNotNull(message.MessageBody);
        }



        //приватные методы
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
