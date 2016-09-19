using Common.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb
{
    public class SignaloBotMongoDbContext
    {
        //поля
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        private IMongoDatabase _database;


        //свойства     
        public IMongoCollection<UserDeliveryTypeSettings<ObjectId>> UserDeliveryTypeSettings
        {
            get
            {
                return _database.GetCollection<UserDeliveryTypeSettings<ObjectId>>("UserDeliveryTypeSettings");
            }
        }
        public IMongoCollection<UserCategorySettings<ObjectId>> UserCategorySettings
        {
            get
            {
                return _database.GetCollection<UserCategorySettings<ObjectId>>("UserCategorySettings");
            }
        }
        public IMongoCollection<UserTopicSettings<ObjectId>> UserTopicSettings
        {
            get
            {
                return _database.GetCollection<UserTopicSettings<ObjectId>>("UserTopicSettings");
            }
        }
        public IMongoCollection<UserReceivePeriod<ObjectId>> UserReceivePeriods
        {
            get
            {
                return _database.GetCollection<UserReceivePeriod<ObjectId>>("UserReceivePeriods");
            }
        }
        public IMongoCollection<SignalEventBase<ObjectId>> SignalEvents
        {
            get
            {
                return _database.GetCollection<SignalEventBase<ObjectId>>("SignalEvents");
            }
        }
        public IMongoCollection<SignalDispatchBase<ObjectId>> SignalDispatches
        {
            get
            {
                return _database.GetCollection<SignalDispatchBase<ObjectId>>("SignalDispatches");
            }
        }
        public IMongoCollection<SignalBounce<ObjectId>> SignalBounces
        {
            get
            {
                return _database.GetCollection<SignalBounce<ObjectId>>("SignalBounces");
            }
        }
        public IMongoCollection<ComposerSettings<ObjectId>> ComposerSettings
        {
            get
            {
                return _database.GetCollection<ComposerSettings<ObjectId>>("ComposerSettings");
            }
        }



        //инициализация
        public SignaloBotMongoDbContext(MongoDbConnectionSettings settings)
        {
            _database = GetDatabase(settings);

            lock (_mapLock)
            {
                if (!_isMapped)
                {
                    _isMapped = true;
                    MapSerialization();
                    MapSignals();
                    MapUserSettings();
                    MapComposerSettings();
                }
            }
        }


        //методы
        public static void ApplyGlobalSerializationSettings()
        {
            //сериализаторы
            var dateSerializer = new DateTimeSerializer(DateTimeKind.Utc);
            BsonSerializer.RegisterSerializer(typeof(DateTime), dateSerializer);

            //проверка Id
            BsonSerializer.UseNullIdChecker = true;
            BsonSerializer.UseZeroIdChecker = true;
        }

        private IMongoDatabase GetDatabase(MongoDbConnectionSettings settings)
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(settings.Host, settings.Port),
                WriteConcern = WriteConcern.Acknowledged,
                ReadPreference = ReadPreference.PrimaryPreferred
            };

            if (!string.IsNullOrEmpty(settings.Login) && !string.IsNullOrEmpty(settings.Password))
            {
                string authDatabase = string.IsNullOrEmpty(settings.AuthSource) ? settings.DatabaseName : settings.AuthSource;

                clientSettings.Credentials = new[]
                {
                    MongoCredential.CreateMongoCRCredential(authDatabase, settings.Login, settings.Password)
                };
            }
            
            MongoClient client = new MongoClient(clientSettings);
            return client.GetDatabase(settings.DatabaseName);
        }

        private void MapSerialization()
        {
            //соглашения
            var pack = new ConventionPack();
            pack.Add(new EnumRepresentationConvention(BsonType.Int32));
            pack.Add(new IgnoreIfNullConvention(true));
            pack.Add(new IgnoreIfDefaultConvention(false));

            Assembly thisAssembly = typeof(SignaloBotMongoDbContext).Assembly;
            Assembly dalAssembly = typeof(SignalDispatchBase<>).Assembly;
            ConventionRegistry.Register("SignaloBot custom pack",
                pack,
                t => t.Assembly == thisAssembly || t.Assembly == dalAssembly);
        }

        private void MapSignals()
        {
            BsonClassMap.RegisterClassMap<SignalEventBase<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalEventID));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SignalDispatchBase<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalDispatchID));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SubjectDispatch<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalDispatchID));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<SignalBounce<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.SignalBounceID));
                cm.SetIgnoreExtraElements(true);
            });
        }

        private void MapUserSettings()
        {
            BsonClassMap.RegisterClassMap<UserDeliveryTypeSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<UserCategorySettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<UserTopicSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<UserReceivePeriod<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.UnmapProperty(p => p.PeriodBeginString);
                cm.UnmapProperty(p => p.PeriodEndString);
            });
        }

        private void MapComposerSettings()
        {
            BsonClassMap.RegisterClassMap<ComposerSettings<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.ComposerSettingsID));
            });

            BsonClassMap.RegisterClassMap<SubscribtionParameters>(cm =>
            {
                cm.AutoMap();
                cm.UnmapProperty(p => p.SelectFromCategories);
                cm.UnmapProperty(p => p.SelectFromTopics);
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
