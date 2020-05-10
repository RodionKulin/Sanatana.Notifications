using Autofac;
using MongoDB.Bson;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.MongoDb.Entities;
using Sanatana.Notifications.DAL.MongoDb.Formatting;
using Sanatana.Notifications.DAL.MongoDb.Queries;
using Sanatana.Notifications.DAL.Queries;

namespace Sanatana.Notifications.DAL.MongoDb.DI.Autofac
{
    /// <summary>
    /// Register MongoDb implementation for core database queries. 
    /// Is required to use MongoDb for database queries.
    /// </summary>
    public class MongoDbSenderAutofacModule<TDeliveryType, TCategory, TTopic> : Module
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>, new()
        where TCategory : SubscriberCategorySettings<ObjectId>, new()
        where TTopic : SubscriberTopicSettings<ObjectId>, new()
    {
        private MongoDbConnectionSettings _connectionSettings;


        public MongoDbSenderAutofacModule(MongoDbConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_connectionSettings).AsSelf().SingleInstance();
            builder.RegisterType<SenderMongoDbContext<TDeliveryType, TCategory, TTopic>>()
                .AsSelf()
                .As<ICollectionFactory>()
                .SingleInstance();

            builder.RegisterType<SenderMongoDbInitializer<TDeliveryType, TCategory, TTopic>>().AsSelf().SingleInstance();

            builder.RegisterType<ObjectIdFileRepository>().As<IFileRepository>().SingleInstance();

            builder.RegisterType<MongoDbEventSettingsQueries>().As<IEventSettingsQueries<ObjectId>>().SingleInstance();

            builder.RegisterType<MongoDbConsolidationLockQueries>().As<IConsolidationLockQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSignalBounceQueries>().As<ISignalBounceQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSignalDispatchHistoryQueries>().As<ISignalDispatchHistoryQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSignalDispatchQueries>().As<ISignalDispatchQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSignalEventQueries>().As<ISignalEventQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbStoredNotificationQueries>().As<IStoredNotificationQueries<ObjectId>>().SingleInstance();

            builder.RegisterType<MongoDbSubscriberQueries<TDeliveryType, TCategory, TTopic>>().As<ISubscriberQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberCategorySettingsQueries<TCategory>>().As<ISubscriberCategorySettingsQueries<TCategory, ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberDeliveryTypeSettingsQueries<TDeliveryType, TCategory>>().As<ISubscriberDeliveryTypeSettingsQueries<TDeliveryType, ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberScheduleSettingsQueries>().As<ISubscriberScheduleSettingsQueries<ObjectId>>().SingleInstance();
            builder.RegisterType<MongoDbSubscriberTopicSettingsQueries<TTopic>>().As<ISubscriberTopicSettingsQueries<TTopic, ObjectId>>().SingleInstance();
        }
    }
}
