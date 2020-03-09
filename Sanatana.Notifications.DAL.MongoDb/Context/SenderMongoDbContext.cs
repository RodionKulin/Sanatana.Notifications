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
using Sanatana.Notifications.DeliveryTypes.Slack;
using Sanatana.Notifications.DAL.MongoDb.Entities;

namespace Sanatana.Notifications.DAL.MongoDb.Context
{
    public class SenderMongoDbContext<TDeliveryType, TCategory, TTopic> : ICollectionFactory
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>
        where TCategory : SubscriberCategorySettings<ObjectId>
        where TTopic : SubscriberTopicSettings<ObjectId>
    {
        //fields
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        private IMongoDatabase _database;
        private MongoDbConnectionSettings _settings;

        private Dictionary<Type, string> _collectionNames = new Dictionary<Type, string>
        {
            { typeof(EventSettings<ObjectId>), "EventSettings" },
            { typeof(DispatchTemplate<ObjectId>), "DispatchTemplates" },
            { typeof(SignalEvent<ObjectId>), "SignalEvents" },
            { typeof(SignalDispatch<ObjectId>), "SignalDispatches" },
            { typeof(StoredNotification<ObjectId>), "StoredNotifications" },
            { typeof(SignalBounce<ObjectId>), "SignalBounces" },
            { typeof(TDeliveryType), "SubscriberDeliveryTypeSettings" },
            { typeof(TCategory), "SubscriberCategorySettings" },
            { typeof(TTopic), "SubscriberTopicSettings" },
            { typeof(SubscriberScheduleSettings<ObjectId>), "SubscriberScheduleSettings" }
        };


        //properties     
        public virtual IMongoCollection<EventSettings<ObjectId>> EventSettings
        {
            get
            {
                return GetCollection<EventSettings<ObjectId>>();
            }
        }
        public virtual IMongoCollection<DispatchTemplate<ObjectId>> DispatchTemplates
        {
            get
            {
                return GetCollection<DispatchTemplate<ObjectId>>();
            }
        }
        public virtual IMongoCollection<SignalEvent<ObjectId>> SignalEvents
        {
            get
            {
                return GetCollection<SignalEvent<ObjectId>>();
            }
        }
        public virtual IMongoCollection<SignalDispatch<ObjectId>> SignalDispatches
        {
            get
            {
                return GetCollection<SignalDispatch<ObjectId>>();
            }
        }
        public virtual IMongoCollection<StoredNotification<ObjectId>> StoredNotifications
        {
            get
            {
                return GetCollection<StoredNotification<ObjectId>>();
            }
        }
        public virtual IMongoCollection<SignalBounce<ObjectId>> SignalBounces
        {
            get
            {
                return GetCollection<SignalBounce<ObjectId>>();
            }
        }
        public virtual IMongoCollection<MongoDbSubscriberDeliveryTypeSettings<TCategory>> SubscriberDeliveryTypeSettings
        {
            get
            {
                return GetCollection<MongoDbSubscriberDeliveryTypeSettings<TCategory>>();
            }
        }
        public virtual IMongoCollection<SubscriberCategorySettings<ObjectId>> SubscriberCategorySettings
        {
            get
            {
                return GetCollection<SubscriberCategorySettings<ObjectId>>();
            }
        }
        public virtual IMongoCollection<SubscriberTopicSettings<ObjectId>> SubscriberTopicSettings
        {
            get
            {
                return GetCollection<SubscriberTopicSettings<ObjectId>>();
            }
        }
        public virtual IMongoCollection<SubscriberScheduleSettings<ObjectId>> SubscriberScheduleSettings
        {
            get
            {
                return GetCollection<SubscriberScheduleSettings<ObjectId>>();
            }
        }


        //init
        public SenderMongoDbContext(MongoDbConnectionSettings settings)
        {
            _settings = settings;
            _database = GetDatabase(settings);

            if (!_isMapped)
            {
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

            Assembly mongoDbDalAssembly = typeof(IEnumerableExtensions).Assembly;
            Assembly dalAssembly = typeof(SignalDispatch<>).Assembly;
            ConventionRegistry.Register("Notifications pack",
                pack,
                t => t.Assembly == mongoDbDalAssembly || t.Assembly == dalAssembly);
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

            BsonClassMap.RegisterClassMap<SlackDispatch<ObjectId>>(cm =>
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

            BsonClassMap.RegisterClassMap<MongoDbSubscriberDeliveryTypeSettings<TCategory>>(cm =>
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



        //ICollectionFactory
        public virtual IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            Type entityType = typeof(TEntity);
            if (!_collectionNames.ContainsKey(entityType))
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered as a MongoDb collection. First add an entry to {nameof(_collectionNames)}");
            }

            string collectionName = _collectionNames[entityType];
            IMongoCollection<TEntity> collection = _database.GetCollection<TEntity>(
                _settings.CollectionsPrefix + collectionName);
            return collection;           
        }
    }
}
