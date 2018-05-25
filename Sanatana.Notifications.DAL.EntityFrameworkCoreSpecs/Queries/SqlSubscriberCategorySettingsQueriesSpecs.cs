using NUnit.Framework;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;
using SpecsFor.ShouldExtensions;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.Queries
{
    public class SqlSubscriberCategorySettingsQueriesSpecs
    {

        [TestFixture]
        public class when_inserting_categories_using_ef 
            : SpecsFor<SqlSubscriberCategorySettingsQueries>, INeedDbContext
        {
            private List<SubscriberCategorySettings<long>> _insertedData;
            public SenderDbContext DbContext { get; set; }


            
            protected override void When()
            {
                _insertedData = new List<SubscriberCategorySettings<long>>
                {
                    new SubscriberCategorySettings<long>()
                    {
                        CategoryId = 1,
                        DeliveryType = 4,
                        IsEnabled = true,
                        SubscriberId = 10
                    },
                    new SubscriberCategorySettings<long>()
                    {
                        CategoryId = 1,
                        DeliveryType = 2,
                        IsEnabled = true,
                        SubscriberId = 10
                    }
                };

                SUT.UpsertIsEnabled(_insertedData).Wait();
            }


            [Test]
            public void then_inserts_category_settings_using_ef()
            {
                List<SubscriberCategorySettingsLong> actual = DbContext.SubscriberCategorySettings
                   .Where(x => x.SubscriberId == 10)
                   .OrderBy(x => x.SubscriberCategorySettingsId)
                   .ToList();

                actual.ShouldNotBeEmpty();
                actual.Count.ShouldEqual(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    SubscriberCategorySettingsLong actualItem = actual[i];
                    actualItem.ShouldLookLikePartial(new
                    {
                        CategoryId = 1,
                        IsEnabled = true,
                    });
                }
            }

        }


    }
}
