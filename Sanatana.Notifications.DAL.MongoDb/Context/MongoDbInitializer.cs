using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbInitializer
    {
        //properties
        public SenderMongoDbContext Context { get; set; }


        //init
        public MongoDbInitializer(MongoDbConnectionSettings settings)
        {
            Context = new SenderMongoDbContext(settings);
        }


        //methods
        public void CreateAllIndexes(bool useGroupId)
        {
            CreateSubscriberDeliveryTypeSettingsIndex();
            if(useGroupId)
            {
                CreateSubscriberDeliveryTypeSettingsGroupIdIndex();
            }

            CreateSubscriberCategorySettingsIndex(useGroupId);
            CreateSubscriberTopicSettingsIndex();
            CreateSubscriberReceivePeriodsIndex();
            CreateSignalEventIndex();
            CreateSignalDispatchIndex();
            CreateSignalBounceIndex();
            CreateEventSettingsIndex();
        }

        public void CreateSubscriberDeliveryTypeSettingsIndex()
        {
            IndexKeysDefinition<SubscriberDeliveryTypeSettings<ObjectId>> subscriberIndex = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };


            IndexKeysDefinition<SubscriberDeliveryTypeSettings<ObjectId>> addressIndex = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.Address);

            CreateIndexOptions addressOptions = new CreateIndexOptions()
            {
                Name = "Address",
                Unique = false
            };


            IMongoCollection<SubscriberDeliveryTypeSettings<ObjectId>> collection = Context.SubscriberDeliveryTypeSettings;
            collection.Indexes.DropAllAsync().Wait();

            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;
            string addressName = collection.Indexes.CreateOneAsync(addressIndex, addressOptions).Result;
        }
        public void CreateSubscriberDeliveryTypeSettingsGroupIdIndex()
        {
            IndexKeysDefinition<SubscriberDeliveryTypeSettings<ObjectId>> groupIdIndex = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.GroupId);

            CreateIndexOptions groupIdOptions = new CreateIndexOptions()
            {
                Name = "GroupId",
                Unique = false
            };
            

            IMongoCollection<SubscriberDeliveryTypeSettings<ObjectId>> collection = Context.SubscriberDeliveryTypeSettings;
            collection.Indexes.DropAllAsync().Wait();

            string subscriberName = collection.Indexes.CreateOneAsync(groupIdIndex, groupIdOptions).Result;
        }
        public void CreateSubscriberCategorySettingsIndex(bool useGroupId)
        {
            IndexKeysDefinition<SubscriberCategorySettings<ObjectId>> subscriberIndex = Builders<SubscriberCategorySettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };

            IndexKeysDefinition<SubscriberCategorySettings<ObjectId>> categoryIndex = null;            
            if(useGroupId)
            {
                categoryIndex = Builders<SubscriberCategorySettings<ObjectId>>.IndexKeys
                    .Ascending(p => p.GroupId)
                    .Ascending(p => p.CategoryId);
            }
            else
            {
                categoryIndex = Builders<SubscriberCategorySettings<ObjectId>>.IndexKeys
                    .Ascending(p => p.CategoryId);
            }

            CreateIndexOptions categoryOptions = new CreateIndexOptions()
            {
                Name = "CategoryId",
                Unique = false
            };

            IMongoCollection<SubscriberCategorySettings<ObjectId>> collection = Context.SubscriberCategorySettings;
            collection.Indexes.DropAllAsync().Wait();

            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;
            string categoryName = collection.Indexes.CreateOneAsync(categoryIndex, categoryOptions).Result;

        }
        public void CreateSubscriberTopicSettingsIndex()
        {
            IndexKeysDefinition<SubscriberTopicSettings<ObjectId>> topicIndex = Builders<SubscriberTopicSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.CategoryId)
                .Ascending(p => p.TopicId);

            CreateIndexOptions topicOptions = new CreateIndexOptions()
            {
                Name = "CategoryId + TopicId",
                Unique = false
            };
            
            IndexKeysDefinition<SubscriberTopicSettings<ObjectId>> subscriberIndex = Builders<SubscriberTopicSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };


            IMongoCollection<SubscriberTopicSettings<ObjectId>> collection = Context.SubscriberTopicSettings;
            collection.Indexes.DropAllAsync().Wait();

            string topicName = collection.Indexes.CreateOneAsync(topicIndex, topicOptions).Result;
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;

        }
        public void CreateSubscriberReceivePeriodsIndex()
        {
            IndexKeysDefinition<SubscriberScheduleSettings<ObjectId>> subscriberIndex = Builders<SubscriberScheduleSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };

            IMongoCollection<SubscriberScheduleSettings<ObjectId>> collection = Context.SubscriberReceivePeriods;
            collection.Indexes.DropAllAsync().Wait();

            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;
        }
        public void CreateSignalEventIndex()
        {
            var sendDateIndex = Builders<SignalEvent<ObjectId>>.IndexKeys
               .Ascending(p => p.CreateDateUtc)
               .Ascending(p => p.FailedAttempts);

            CreateIndexOptions failedAttemptsOptions = new CreateIndexOptions()
            {
                Name = "FailedAttempts",
                Unique = false
            };
                        
            IMongoCollection<SignalEvent<ObjectId>> collection = Context.SignalEvents;
            collection.Indexes.DropAllAsync().Wait();

            string failedAttemptsName = collection.Indexes.CreateOneAsync(sendDateIndex, failedAttemptsOptions).Result;
        }
        public void CreateSignalDispatchIndex()
        {
            var sendDateIndex = Builders<SignalDispatch<ObjectId>>.IndexKeys
               .Ascending(p => p.SendDateUtc)
               .Ascending(p => p.FailedAttempts);

            CreateIndexOptions sendDateOptions = new CreateIndexOptions()
            {
                Name = "SendDateUtc + FailedAttempts",
                Unique = false
            };


            var receiverIndex = Builders<SignalDispatch<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiverSubscriberId)
               .Ascending(p => p.SendDateUtc);

            CreateIndexOptions receiverOptions = new CreateIndexOptions()
            {
                Name = "ReceiverSubscriberId + SendDateUtc",
                Unique = false
            };


            IMongoCollection<SignalDispatch<ObjectId>> collection = Context.SignalDispatches;
            collection.Indexes.DropAllAsync().Wait();

            string sendDateName = collection.Indexes.CreateOneAsync(sendDateIndex, sendDateOptions).Result;
            string receiverName = collection.Indexes.CreateOneAsync(receiverIndex, receiverOptions).Result;

        }
        public void CreateSignalBounceIndex()
        {
            IndexKeysDefinition<SignalBounce<ObjectId>> subscriberIndex = Builders<SignalBounce<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiverSubscriberId)
               .Ascending(p => p.BounceReceiveDateUtc);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "ReceiverSubscriberId + BounceReceiveDateUtc",
                Unique = false
            };


            IMongoCollection<SignalBounce<ObjectId>> collection = Context.SignalBounces;
            collection.Indexes.DropAllAsync().Wait();

            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;

        }
        public void CreateEventSettingsIndex()
        {
            IndexKeysDefinition<EventSettings<ObjectId>> subscriberIndex = Builders<EventSettings<ObjectId>>.IndexKeys
               .Ascending(p => p.CategoryId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "CategoryId",
                Unique = false
            };


            IMongoCollection<EventSettings<ObjectId>> collection = Context.EventSettings;
            collection.Indexes.DropAllAsync().Wait();

            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;

        }
    }
}
