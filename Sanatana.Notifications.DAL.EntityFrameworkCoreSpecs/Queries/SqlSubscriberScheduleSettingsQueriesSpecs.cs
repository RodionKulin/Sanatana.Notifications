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
    public class SqlSubscriberScheduleSettingsQueriesSpecs
    {
        [TestFixture]
        public class when_schedule_settings_rewriting_sets_using_ef
           : SpecsFor<SqlSubscriberScheduleSettingsQueries>, INeedDbContext
        {
            private List<SubscriberScheduleSettings<long>> _insertedData;
            private long _subscriberId;
            public SenderDbContext DbContext { get; set; }



            protected override void When()
            {
                _subscriberId = 11;
                _insertedData = new List<SubscriberScheduleSettings<long>>
                {
                    new SubscriberScheduleSettings<long>
                    {
                        Order = 1,
                        PeriodBegin = TimeSpan.FromHours(1),
                        PeriodEnd = TimeSpan.FromHours(2),
                        Set = 1,
                        SubscriberId = _subscriberId
                    },
                    new SubscriberScheduleSettings<long>
                    {
                        Order = 2,
                        PeriodBegin = TimeSpan.FromHours(13),
                        PeriodEnd = TimeSpan.FromHours(14),
                        Set = 1,
                        SubscriberId = _subscriberId
                    }
                };

                SUT.RewriteSets(_subscriberId, _insertedData).Wait();
            }


            [Test]
            public void then_schedule_settings_rewriten_sets_are_found_using_ef()
            {
                List<SubscriberScheduleSettingsLong> actual = DbContext.SubscriberScheduleSettings
                   .Where(x => x.SubscriberId == _subscriberId)
                   .OrderBy(x => x.Set)
                   .ThenBy(x => x.Order)
                   .ToList();

                actual.ShouldNotBeEmpty();
                actual.Count.ShouldEqual(_insertedData.Count);

                for (int i = 0; i < _insertedData.Count; i++)
                {
                    SubscriberScheduleSettingsLong actualItem = actual[i];
                    SubscriberScheduleSettings<long> expectedItem = _insertedData[i];

                    actualItem.Order.ShouldEqual(expectedItem.Order);
                    actualItem.PeriodBegin.ShouldEqual(expectedItem.PeriodBegin);
                    actualItem.PeriodEnd.ShouldEqual(expectedItem.PeriodEnd);
                    actualItem.Set.ShouldEqual(expectedItem.Set);
                }
            }

        }



    }
}
