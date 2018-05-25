using Autofac;
using MongoDB.Bson;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.DI.Autofac
{
    /// <summary>
    /// Register MongoDb implementation for database queries. 
    /// Is required to use MongoDb for database queries.
    /// </summary>
    public class MongoDbAutofacModule : Module
    {
        private MongoDbConnectionSettings _connectionSettings;


        public MongoDbAutofacModule(MongoDbConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_connectionSettings).AsSelf().SingleInstance();

            builder.RegisterType<MongoDbSignalBounceQueries>().As<ISignalBounceQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSignalDispatchQueries>().As<ISignalDispatchQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSignalEventQueries>().As<ISignalEventQueries<ObjectId>>().SingleInstance();

            builder.RegisterType<MongoDbSubscriberQueries>().As<ISubscriberQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberCategorySettingsQueries>().As<ISubscriberCategorySettingsQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberDeliveryTypeSettingsQueries>().As<ISubscriberDeliveryTypeSettingsQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberScheduleSettingsQueries>().As<ISubscriberScheduleSettingsQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberTopicSettingsQueries>().As<ISubscriberTopicSettingsQueries<ObjectId>>().SingleInstance();
        }
    }
}
