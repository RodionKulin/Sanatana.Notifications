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
using System.Diagnostics;
using TemplatesExample;

namespace Sanatana.NotificationsTests.EventsHandling.Templates
{
    [TestClass()]
    public class XsltTransformerTests
    {
        [TestMethod()]
        public void XsltTransformer_TransformTest()
        {
            //prepare
            var templateProvider = new FileTemplate("TestTools/Content/ProductsOrder.xslt");
            var templateData = new List<TemplateData>
            {
                new TemplateData(keyValueModel: null, objectModel: CreateObjectModel())
                
            };

            //invoke
            Stopwatch timer = Stopwatch.StartNew();
            var target = new XsltTransformer();
            Dictionary<string, string> filledTemplates = target.Transform(templateProvider, templateData);

            //assert
            Debug.WriteLine("Xslt time: " + timer.Elapsed);
            Assert.AreEqual(1, filledTemplates.Count);
            Assert.IsNotNull(filledTemplates.Values.First());
        }


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
