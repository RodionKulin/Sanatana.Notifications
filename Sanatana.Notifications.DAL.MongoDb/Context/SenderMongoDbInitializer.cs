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
    public class SenderMongoDbInitializer
    {
        //properties
        public SenderMongoDbContext Context { get; set; }


        //init
        public SenderMongoDbInitializer(MongoDbConnectionSettings settings)
        {
            Context = new SenderMongoDbContext(settings);
        }


        //methods
        public virtual void CreateAllIndexes(bool useGroupId)
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
            CreateStoredNotificationIndex();
            CreateSignalBounceIndex();
            CreateEventSettingsIndex();
            CreateDispatchTemplateIndex();
        }
        public virtual void DropAllIndexes()
        {
            Context.SubscriberDeliveryTypeSettings.Indexes.DropAllAsync().Wait();
            Context.SubscriberCategorySettings.Indexes.DropAllAsync().Wait();
            Context.SubscriberTopicSettings.Indexes.DropAllAsync().Wait();
            Context.SubscriberReceivePeriods.Indexes.DropAllAsync().Wait();
            Context.SignalEvents.Indexes.DropAllAsync().Wait();
            Context.SignalDispatches.Indexes.DropAllAsync().Wait();
            Context.StoredNotifications.Indexes.DropAllAsync().Wait();
            Context.SignalBounces.Indexes.DropAllAsync().Wait();
            Context.EventSettings.Indexes.DropAllAsync().Wait();
            Context.DispatchTemplates.Indexes.DropAllAsync().Wait();
        }


        public virtual void CreateSubscriberDeliveryTypeSettingsIndex()
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
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;
            string addressName = collection.Indexes.CreateOneAsync(addressIndex, addressOptions).Result;
        }
        public virtual void CreateSubscriberDeliveryTypeSettingsGroupIdIndex()
        {
            IndexKeysDefinition<SubscriberDeliveryTypeSettings<ObjectId>> groupIdIndex = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.GroupId);

            CreateIndexOptions groupIdOptions = new CreateIndexOptions()
            {
                Name = "GroupId",
                Unique = false
            };
            

            IMongoCollection<SubscriberDeliveryTypeSettings<ObjectId>> collection = Context.SubscriberDeliveryTypeSettings;
            string subscriberName = collection.Indexes.CreateOneAsync(groupIdIndex, groupIdOptions).Result;
        }
        public virtual void CreateSubscriberCategorySettingsIndex(bool useGroupId)
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
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;
            string categoryName = collection.Indexes.CreateOneAsync(categoryIndex, categoryOptions).Result;

        }
        public virtual void CreateSubscriberTopicSettingsIndex()
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
            string topicName = collection.Indexes.CreateOneAsync(topicIndex, topicOptions).Result;
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;

        }
        public virtual void CreateSubscriberReceivePeriodsIndex()
        {
            IndexKeysDefinition<SubscriberScheduleSettings<ObjectId>> subscriberIndex = Builders<SubscriberScheduleSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };

            IMongoCollection<SubscriberScheduleSettings<ObjectId>> collection = Context.SubscriberReceivePeriods;
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;
        }
        public virtual void CreateSignalEventIndex()
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
            string failedAttemptsName = collection.Indexes.CreateOneAsync(sendDateIndex, failedAttemptsOptions).Result;
        }
        public virtual void CreateSignalDispatchIndex()
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
            string sendDateName = collection.Indexes.CreateOneAsync(sendDateIndex, sendDateOptions).Result;
            string receiverName = collection.Indexes.CreateOneAsync(receiverIndex, receiverOptions).Result;

        }
        public virtual void CreateStoredNotificationIndex()
        {
            var subscriberIndex = Builders<StoredNotification<ObjectId>>.IndexKeys
               .Ascending(p => p.SubscriberId)
               .Ascending(p => p.CreateDateUtc);

            CreateIndexOptions sendDateOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId + CreateDateUtc",
                Unique = false
            };

            IMongoCollection<StoredNotification<ObjectId>> collection = Context.StoredNotifications;
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, sendDateOptions).Result;

        }
        public virtual void CreateSignalBounceIndex()
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
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;

        }
        public virtual void CreateEventSettingsIndex()
        {
            IndexKeysDefinition<EventSettings<ObjectId>> subscriberIndex = Builders<EventSettings<ObjectId>>.IndexKeys
               .Ascending(p => p.CategoryId);

            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "CategoryId",
                Unique = false
            };

            IMongoCollection<EventSettings<ObjectId>> collection = Context.EventSettings;
            string subscriberName = collection.Indexes.CreateOneAsync(subscriberIndex, subscriberOptions).Result;

        }
        public virtual void CreateDispatchTemplateIndex()
        {
            //no extra indexes requied
        }
    }
}
