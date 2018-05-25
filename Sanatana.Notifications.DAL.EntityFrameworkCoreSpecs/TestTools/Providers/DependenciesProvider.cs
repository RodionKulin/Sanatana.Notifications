using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using SpecsFor.Configuration;
using System.IO;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using System.Configuration;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DependenciesProvider : Behavior<INeedDbContext>
    {
        //methods
        public override void SpecInit(INeedDbContext instance)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SenderDbContext"].ConnectionString;
            var connection = new SqlConnectionSettings
            {
                ConnectionString = connectionString,
                Schema = "dbo"
            };

            instance.MockContainer.Configure(cfg =>
            {
                cfg.For<SqlConnectionSettings>().Use(connection);
                cfg.For<ISenderDbContextFactory>().Use<SenderDbContextFactory>();
                cfg.For<INotificationsMapperFactory>().Use<NotificationsMapperFactory>();

                cfg.For<SqlSignalBounceQueries>().Use<SqlSignalBounceQueries>();
                cfg.For<SqlSignalDispatchQueries>().Use<SqlSignalDispatchQueries>();
                cfg.For<SqlSignalEventQueries>().Use<SqlSignalEventQueries>();
                cfg.For<SqlStoredNotificationQueries>().Use<SqlStoredNotificationQueries>();
                cfg.For<SqlSubscriberQueries>().Use<SqlSubscriberQueries>();
                cfg.For<SqlSubscriberCategorySettingsQueries>().Use<SqlSubscriberCategorySettingsQueries>();
                cfg.For<SqlSubscriberDeliveryTypeSettingsQueries>().Use<SqlSubscriberDeliveryTypeSettingsQueries>();
                cfg.For<SqlSubscriberScheduleSettingsQueries>().Use<SqlSubscriberScheduleSettingsQueries>();
                cfg.For<SqlSubscriberTopicSettingsQueries>().Use<SqlSubscriberTopicSettingsQueries>();
            });

            ISenderDbContextFactory factory =  instance.MockContainer.GetInstance<ISenderDbContextFactory>();
            instance.DbContext = factory.GetDbContext();
            instance.DbContext.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

        }
    }
}
