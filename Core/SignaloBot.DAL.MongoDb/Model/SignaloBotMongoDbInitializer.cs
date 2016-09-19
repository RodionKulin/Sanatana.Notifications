using Common.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb
{
    public class SignaloBotMongoDbInitializer
    {
        //свойства
        public SignaloBotMongoDbContext Context { get; set; }


        //инициализация
        public SignaloBotMongoDbInitializer(MongoDbConnectionSettings settings)
        {
            Context = new SignaloBotMongoDbContext(settings);
        }


        //методы
        public void CreateAllIndexes(bool useGroupID)
        {
            CreateUserDeliveryTypeSettingsIndex();
            if(useGroupID)
            {
                CreateUserDeliveryTypeSettingsGroupIDIndex();
            }

            CreateUserCategorySettingsIndex(useGroupID);
            CreateUserTopicSettingsIndex();
            CreateUserReceivePeriodsIndex();
            CreateSignalEventIndex();
            CreateSignalDispatchIndex();
            CreateSignalBounceIndex();
            CreateComposerSettingsIndex();
        }

        public void CreateUserDeliveryTypeSettingsIndex()
        {
            IndexKeysDefinition<UserDeliveryTypeSettings<ObjectId>> userIndex = Builders<UserDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.UserID);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "UserID",
                Unique = false
            };


            IndexKeysDefinition<UserDeliveryTypeSettings<ObjectId>> addressIndex = Builders<UserDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.Address);

            CreateIndexOptions addressOptions = new CreateIndexOptions()
            {
                Name = "Address",
                Unique = false
            };


            IMongoCollection<UserDeliveryTypeSettings<ObjectId>> collection = Context.UserDeliveryTypeSettings;
            collection.Indexes.DropAllAsync().Wait();

            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;
            string addressName = collection.Indexes.CreateOneAsync(addressIndex, addressOptions).Result;
        }
        public void CreateUserDeliveryTypeSettingsGroupIDIndex()
        {
            IndexKeysDefinition<UserDeliveryTypeSettings<ObjectId>> groupIdIndex = Builders<UserDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.GroupID);

            CreateIndexOptions groupIdOptions = new CreateIndexOptions()
            {
                Name = "GroupID",
                Unique = false
            };
            

            IMongoCollection<UserDeliveryTypeSettings<ObjectId>> collection = Context.UserDeliveryTypeSettings;
            collection.Indexes.DropAllAsync().Wait();

            string userName = collection.Indexes.CreateOneAsync(groupIdIndex, groupIdOptions).Result;
        }
        public void CreateUserCategorySettingsIndex(bool useGroupID)
        {
            IndexKeysDefinition<UserCategorySettings<ObjectId>> userIndex = Builders<UserCategorySettings<ObjectId>>.IndexKeys
                .Ascending(p => p.UserID);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "UserID",
                Unique = false
            };

            IndexKeysDefinition<UserCategorySettings<ObjectId>> categoryIndex = null;            
            if(useGroupID)
            {
                categoryIndex = Builders<UserCategorySettings<ObjectId>>.IndexKeys
                    .Ascending(p => p.GroupID)
                    .Ascending(p => p.CategoryID);
            }
            else
            {
                categoryIndex = Builders<UserCategorySettings<ObjectId>>.IndexKeys
                    .Ascending(p => p.CategoryID);
            }

            CreateIndexOptions categoryOptions = new CreateIndexOptions()
            {
                Name = "CategoryID",
                Unique = false
            };

            IMongoCollection<UserCategorySettings<ObjectId>> collection = Context.UserCategorySettings;
            collection.Indexes.DropAllAsync().Wait();

            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;
            string categoryName = collection.Indexes.CreateOneAsync(categoryIndex, categoryOptions).Result;

        }
        public void CreateUserTopicSettingsIndex()
        {
            IndexKeysDefinition<UserTopicSettings<ObjectId>> topicIndex = Builders<UserTopicSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.CategoryID)
                .Ascending(p => p.TopicID);

            CreateIndexOptions topicOptions = new CreateIndexOptions()
            {
                Name = "CategoryID + TopicID",
                Unique = false
            };
            
            IndexKeysDefinition<UserTopicSettings<ObjectId>> userIndex = Builders<UserTopicSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.UserID);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "UserID",
                Unique = false
            };


            IMongoCollection<UserTopicSettings<ObjectId>> collection = Context.UserTopicSettings;
            collection.Indexes.DropAllAsync().Wait();

            string topicName = collection.Indexes.CreateOneAsync(topicIndex, topicOptions).Result;
            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;

        }
        public void CreateUserReceivePeriodsIndex()
        {
            IndexKeysDefinition<UserReceivePeriod<ObjectId>> userIndex = Builders<UserReceivePeriod<ObjectId>>.IndexKeys
                .Ascending(p => p.UserID)
                .Ascending(p => p.ReceivePeriodsGroupID);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "UserID + ReceivePeriodsGroupID",
                Unique = false
            };


            IMongoCollection<UserReceivePeriod<ObjectId>> collection = Context.UserReceivePeriods;
            collection.Indexes.DropAllAsync().Wait();

            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;
        }
        public void CreateSignalEventIndex()
        {
            var sendDateIndex = Builders<SignalEventBase<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiveDateUtc)
               .Ascending(p => p.FailedAttempts);

            CreateIndexOptions receiveDateOptions = new CreateIndexOptions()
            {
                Name = "ReceiveDateUtc + CategoryID + FailedAttempts",
                Unique = false
            };
                        
            IMongoCollection<SignalEventBase<ObjectId>> collection = Context.SignalEvents;
            collection.Indexes.DropAllAsync().Wait();

            string receiveDateName = collection.Indexes.CreateOneAsync(sendDateIndex, receiveDateOptions).Result;
        }
        public void CreateSignalDispatchIndex()
        {
            var sendDateIndex = Builders<SignalDispatchBase<ObjectId>>.IndexKeys
               .Ascending(p => p.SendDateUtc)
               .Ascending(p => p.DeliveryType)
               .Ascending(p => p.FailedAttempts);

            CreateIndexOptions sendDateOptions = new CreateIndexOptions()
            {
                Name = "SendDateUtc + DeliveryType + FailedAttempts",
                Unique = false
            };


            var receiverIndex = Builders<SignalDispatchBase<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiverUserID);

            CreateIndexOptions receiverOptions = new CreateIndexOptions()
            {
                Name = "ReceiverUserID",
                Unique = false
            };


            IMongoCollection<SignalDispatchBase<ObjectId>> collection = Context.SignalDispatches;
            collection.Indexes.DropAllAsync().Wait();

            string sendDateName = collection.Indexes.CreateOneAsync(sendDateIndex, sendDateOptions).Result;
            string receiverName = collection.Indexes.CreateOneAsync(receiverIndex, receiverOptions).Result;

        }
        public void CreateSignalBounceIndex()
        {
            IndexKeysDefinition<SignalBounce<ObjectId>> userIndex = Builders<SignalBounce<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiverUserID)
               .Ascending(p => p.BounceReceiveDateUtc);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "ReceiverUserID + BounceReceiveDateUtc",
                Unique = false
            };


            IMongoCollection<SignalBounce<ObjectId>> collection = Context.SignalBounces;
            collection.Indexes.DropAllAsync().Wait();

            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;

        }
        public void CreateComposerSettingsIndex()
        {
            IndexKeysDefinition<ComposerSettings<ObjectId>> userIndex = Builders<ComposerSettings<ObjectId>>.IndexKeys
               .Ascending(p => p.CategoryID);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "CategoryID",
                Unique = false
            };


            IMongoCollection<ComposerSettings<ObjectId>> collection = Context.ComposerSettings;
            collection.Indexes.DropAllAsync().Wait();

            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;

        }
    }
}
