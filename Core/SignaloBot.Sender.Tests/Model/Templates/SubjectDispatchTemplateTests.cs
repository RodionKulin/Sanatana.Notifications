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
using SignaloBot.DAL;
using SignaloBot.Sender.Composers.Templates;
using SignaloBot.Sender.Test.Resource;

namespace SignaloBot.Sender.Composition.Tests
{
    [TestClass()]
    public class SubjectDispatchTemplateTests
    {
        [TestMethod()]
        public void SubjectDispatchTemplate_ProvideContentTest()
        {
            //параметры
            var builder = new SubjectDispatchTemplate<Guid>()
            {
                BodyProvider = new ResourceTemplate(typeof(ContentRes), "ContentKey"),
                BodyTransformer = new ReplaceTransformer()
            };

            var replaceModel = new Dictionary<string, string>()
            {
                { "key", "value" }
            };
            var subscriber = new Subscriber<Guid>(){ UserID = Guid.NewGuid() };
            TemplateData bodyData = new TemplateData(replaceModel);

            //проверка
            SubjectDispatch<Guid> item = (SubjectDispatch<Guid>)builder.Build(subscriber, bodyData);
            Assert.AreEqual(ContentRes.ContentKey, item.MessageBody);
        }
        
        [TestMethod()]
        public void SubjectDispatchTemplate_ProvideXSLTContentTest()
        {          
            //параметры
            var builder = new SubjectDispatchTemplate<Guid>()
            {
                BodyProvider = new FileTemplate("Content/ProductsOrder.xslt"),
                BodyTransformer =  new XslTransformer()
            };

            object objectModel = CreateObjectModel();
            Subscriber<Guid> subscriber = new Subscriber<Guid>() { UserID = Guid.NewGuid() };
            TemplateData bodyData = new TemplateData(objectModel);

            //проверка         
            Stopwatch timer = Stopwatch.StartNew();
            SubjectDispatch<Guid> item = (SubjectDispatch<Guid>)builder.Build(subscriber, bodyData);
            TimeSpan total = timer.Elapsed;
            Assert.IsNotNull(item.MessageBody);
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
