using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.DeliveryTypes.Http;
using Sanatana.Notifications.DeliveryTypes.StoredNotification;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class SenderMongoDbContext
    {
        //fields
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        private IMongoDatabase _database;
        private MongoDbConnectionSettings _settings;


        //properties     
        public virtual IMongoCollection<SubscriberDeliveryTypeSettings<ObjectId>> SubscriberDeliveryTypeSettings
        {
            get
            {
                return _database.GetCollection<SubscriberDeliveryTypeSettings<ObjectId>>(
                    _settings.CollectionsPrefix + "SubscriberDeliveryTypeSettings");
            }
        }
        public virtual IMongoCollection<SubscriberCategorySettings<ObjectId>> SubscriberCategorySettings
        {
            get
            {
                return _database.GetCollection<SubscriberCategorySettings<ObjectId>>(
                    _settings.CollectionsPrefix + "SubscriberCategorySettings");
            }
        }
        public virtual IMongoCollection<SubscriberTopicSettings<ObjectId>> SubscriberTopicSettings
        {
            get
            {
                return _database.GetCollection<SubscriberTopicSettings<ObjectId>>(
                    _settings.CollectionsPrefix + "SubscriberTopicSettings");
            }
        }
        public virtual IMongoCollection<SubscriberScheduleSettings<ObjectId>> SubscriberReceivePeriods
        {
            get
            {
                return _database.GetCollection<SubscriberScheduleSettings<ObjectId>>(
                    _settings.CollectionsPrefix + "SubscriberScheduleSettings");
            }
        }
        public virtual IMongoCollection<SignalEvent<ObjectId>> SignalEvents
        {
            get
            {
                return _database.GetCollection<SignalEvent<ObjectId>>(_settings.CollectionsPrefix + "SignalEvents");
            }
        }
        public virtual IMongoCollection<SignalDispatch<ObjectId>> SignalDispatches
        {
            get
            {
                return _database.GetCollection<SignalDispatch<ObjectId>>(_settings.CollectionsPrefix + "SignalDispatches");
            }
        }
        public virtual IMongoCollection<StoredNotification<ObjectId>> StoredNotifications
        {
            get
            {
                return _database.GetCollection<StoredNotification<ObjectId>>(_settings.CollectionsPrefix + "StoredNotifications");
            }
        }
        public virtual IMongoCollection<SignalBounce<ObjectId>> SignalBounces
        {
            get
            {
                return _database.GetCollection<SignalBounce<ObjectId>>(_settings.CollectionsPrefix + "SignalBounces");
            }
        }
        public virtual IMongoCollection<EventSettings<ObjectId>> EventSettings
        {
            get
            {
                return _database.GetCollection<EventSettings<ObjectId>>(_settings.CollectionsPrefix + "EventSettings");
            }
        }
        public virtual IMongoCollection<DispatchTemplate<ObjectId>> DispatchTemplates
        {
            get
            {
                return _database.GetCollection<DispatchTemplate<ObjectId>>(_settings.CollectionsPrefix + "DispatchTemplates");
            }
        }


        //init
        public SenderMongoDbContext(MongoDbConnectionSettings settings)
        {
            _settings = settings;
            _database = GetDatabase(settings);

            lock (_mapLock)
            {
                if (!_isMapped)
                {
                    _isMapped = true;
                    RegisterConventions();
                    MapSignals();
                    MapSubscriberSettings();
                    MapEventSettings();
                }
            }
        }


        //methods
        public static void ApplyGlobalSerializationSettings()
        {
            var dateSerializer = new DateTimeSerializer(DateTimeKind.Utc);
            BsonSerializer.RegisterSerializer(typeof(DateTime), dateSerializer);
            
            BsonSerializer.UseNullIdChecker = true;
            BsonSerializer.UseZeroIdChecker = true;
        }

        protected virtual IMongoDatabase GetDatabase(MongoDbConnectionSettings settings)
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(settings.Host, settings.Port),
                WriteConcern = WriteConcern.Acknowledged,
                ReadPreference = ReadPreference.PrimaryPreferred,
                Credential = settings.Credential
            };

            MongoClient client = new MongoClient(clientSettings);
            return client.GetDatabase(settings.DatabaseName);
        }

        protected virtual void RegisterConventions()
        {
            var pack = new ConventionPack();
            pack.Add(new EnumRepresentationConvention(BsonType.Int32));
            pack.Add(new IgnoreIfNullConvention(true));
            pack.Add(new IgnoreIfDefaultConvention(false));

            Assembly thisAssembly = typeof(SenderMongoDbContext).Assembly;
            Assembly dalAssembly = typeof(SignalDispatch<>).Assembly;
            ConventionRegistry.Register("Notifications pack",
                pack,
                t => t.Assembly == thisAssembly || t.Assembly == dalAssembly);
        }

        protected virtual void MapSignals()
        {
            BsonClassMap.RegisterClassMap<SignalEvent<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalEventId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SignalDispatch<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalDispatchId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SignalBounce<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalBounceId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<EmailDispatch<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalDispatchId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<HttpDispatch<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalDispatchId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<StoredNotificationDispatch<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalDispatchId));
                cm.SetIgnoreExtraElements(true);
            });

        }

        protected virtual void MapSubscriberSettings()
        {
            BsonClassMap.RegisterClassMap<SubscriberDeliveryTypeSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SubscriberDeliveryTypeSettingsId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SubscriberCategorySettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SubscriberCategorySettingsId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SubscriberTopicSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SubscriberTopicSettingsId));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SubscriberScheduleSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SubscriberScheduleSettingsId));
                cm.SetIgnoreExtraElements(true);
            });
        }

        protected virtual void MapEventSettings()
        {
            BsonClassMap.RegisterClassMap<EventSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.EventSettingsId));
            });

            BsonClassMap.RegisterClassMap<SubscriptionParameters>(cm =>
            {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<UpdateParameters>(cm =>
            {
                cm.AutoMap();
                cm.UnmapProperty(p => p.UpdateDeliveryType);
                cm.UnmapProperty(p => p.UpdateCategory);
                cm.UnmapProperty(p => p.UpdateTopic);
                cm.UnmapProperty(p => p.UpdateAnything);
            });
        }
    }
}
