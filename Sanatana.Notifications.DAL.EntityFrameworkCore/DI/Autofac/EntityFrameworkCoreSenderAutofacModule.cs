using Autofac;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Queries;
using Sanatana.Notifications.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.DI.Autofac
{
    /// <summary>
    /// Register EntityFrameworkCore implementation for core database queries. 
    /// Is required to use EntityFrameworkCore for database queries.
    /// </summary>
    public class EntityFrameworkCoreSenderAutofacModule : Module
    {
        private SqlConnectionSettings _connectionSettings;

        public EntityFrameworkCoreSenderAutofacModule(SqlConnectionSettings connectionSettings = null)
        {
            _connectionSettings = connectionSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if(_connectionSettings != null)
            {
                builder.RegisterInstance(_connectionSettings).AsSelf().SingleInstance();
            }

            builder.RegisterType<NotificationsMapperFactory>().As<INotificationsMapperFactory>().SingleInstance();          
            builder.RegisterType<SenderDbContextFactory>().As<ISenderDbContextFactory>().SingleInstance();

            builder.RegisterType<SqlConsolidationLockQueries>().As<IConsolidationLockQueries<long>>().SingleInstance();
            builder.RegisterType<SqlSignalBounceQueries>().As<ISignalBounceQueries<long>>().SingleInstance();
            builder.RegisterType<SqlSignalDispatchHistoryQueries>().As<ISignalDispatchHistoryQueries<long>>().SingleInstance();
            builder.RegisterType<SqlSignalDispatchQueries>().As<ISignalDispatchQueries<long>>().SingleInstance();
            builder.RegisterType<SqlSignalEventQueries>().As<ISignalEventQueries<long>>().SingleInstance();
            builder.RegisterType<SqlStoredNotificationQueries>().As<IStoredNotificationQueries<long>>().SingleInstance();

            builder.RegisterType<SqlSubscriberQueries>().As<ISubscriberQueries<long>>().SingleInstance();
            builder.RegisterType<SqlSubscriberCategorySettingsQueries>().As<ISubscriberCategorySettingsQueries<SubscriberCategorySettingsLong, long>>().SingleInstance();
            builder.RegisterType<SqlSubscriberDeliveryTypeSettingsQueries>().As<ISubscriberDeliveryTypeSettingsQueries<SubscriberDeliveryTypeSettingsLong, long>>().SingleInstance();
            builder.RegisterType<SqlSubscriberTopicSettingsQueries>().As<ISubscriberTopicSettingsQueries<SubscriberTopicSettingsLong, long>>().SingleInstance();
            builder.RegisterType<SqlSubscriberScheduleSettingsQueries>().As<ISubscriberScheduleSettingsQueries<long>>().SingleInstance();

        }
    }
}
