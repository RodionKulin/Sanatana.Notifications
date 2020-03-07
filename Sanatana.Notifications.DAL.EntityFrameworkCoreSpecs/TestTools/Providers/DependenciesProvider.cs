using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using System.IO;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using System.Configuration;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Interfaces;
using SpecsFor.Core.Configuration;
using SpecsFor.Core;
using SpecsFor.StructureMap;
using StructureMap.AutoMocking;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DependenciesProvider : Behavior<INeedDbContext>
    {
        //methods
        public override void SpecInit(INeedDbContext instance)
        {
            dynamic specsDynamic = instance;
            AutoMockedContainer autoMockContainer = specsDynamic.Mocker.MoqAutoMocker.Container;

            string connectionString = @"Data Source=.\;Initial Catalog=SanatanaNotificationsSpecs;integrated security=true;MultipleActiveResultSets=True;";
            var connection = new SqlConnectionSettings
            {
                ConnectionString = connectionString,
                Schema = "dbo"
            };

            autoMockContainer.Configure(cfg =>
            {
                cfg.For<SqlConnectionSettings>().Use(connection);
                cfg.For<ISenderDbContextFactory>().Use<SenderDbContextFactory>();
                cfg.For<INotificationsMapperFactory>().Use<NotificationsMapperFactory>();

                cfg.For<IDispatchTemplateQueries<long>>().Use<SqlDispatchTemplateQueries>();
                cfg.For<SqlDispatchTemplateQueries>().Use<SqlDispatchTemplateQueries>();
                cfg.For<SqlEventSettingsQueries>().Use<SqlEventSettingsQueries>();

                cfg.For<SqlSignalBounceQueries>().Use<SqlSignalBounceQueries>();
                cfg.For<SqlSignalDispatchQueries>().Use<SqlSignalDispatchQueries>();
                cfg.For<SqlSignalEventQueries>().Use<SqlSignalEventQueries>();
                cfg.For<SqlStoredNotificationQueries>().Use<SqlStoredNotificationQueries>();

                cfg.For<SqlSubscriberCategorySettingsQueries>().Use<SqlSubscriberCategorySettingsQueries>();
                cfg.For<SqlSubscriberDeliveryTypeSettingsQueries>().Use<SqlSubscriberDeliveryTypeSettingsQueries>();
                cfg.For<SqlSubscriberQueries>().Use<SqlSubscriberQueries>();
                cfg.For<SqlSubscriberScheduleSettingsQueries>().Use<SqlSubscriberScheduleSettingsQueries>();
                cfg.For<SqlSubscriberTopicSettingsQueries>().Use<SqlSubscriberTopicSettingsQueries>();
            });

            ISenderDbContextFactory factory = autoMockContainer.GetInstance<ISenderDbContextFactory>();
            instance.DbContext = factory.GetDbContext();
            instance.DbContext.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
        }
    }
}
