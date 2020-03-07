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
            Context.SubscriberDeliveryTypeSettings.Indexes.DropAll();
            Context.SubscriberCategorySettings.Indexes.DropAll();
            Context.SubscriberTopicSettings.Indexes.DropAll();
            Context.SubscriberReceivePeriods.Indexes.DropAll();
            Context.SignalEvents.Indexes.DropAll();
            Context.SignalDispatches.Indexes.DropAll();
            Context.StoredNotifications.Indexes.DropAll();
            Context.SignalBounces.Indexes.DropAll();
            Context.EventSettings.Indexes.DropAll();
            Context.DispatchTemplates.Indexes.DropAll();
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
            var subscriberModel = new CreateIndexModel<SubscriberDeliveryTypeSettings<ObjectId>>(subscriberIndex, subscriberOptions);

            IndexKeysDefinition<SubscriberDeliveryTypeSettings<ObjectId>> addressIndex = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.Address);
            CreateIndexOptions addressOptions = new CreateIndexOptions()
            {
                Name = "Address",
                Unique = false
            };
            var addressModel = new CreateIndexModel<SubscriberDeliveryTypeSettings<ObjectId>>(addressIndex, addressOptions);

            IMongoCollection<SubscriberDeliveryTypeSettings<ObjectId>> collection = Context.SubscriberDeliveryTypeSettings;
            string subscriberName = collection.Indexes.CreateOne(subscriberModel);
            string addressName = collection.Indexes.CreateOne(addressModel);
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
            var groupIdModel = new CreateIndexModel<SubscriberDeliveryTypeSettings<ObjectId>>(groupIdIndex, groupIdOptions);

            IMongoCollection<SubscriberDeliveryTypeSettings<ObjectId>> collection = Context.SubscriberDeliveryTypeSettings;
            string subscriberName = collection.Indexes.CreateOne(groupIdModel);
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
            var subscriberModel = new CreateIndexModel<SubscriberCategorySettings<ObjectId>>(subscriberIndex, subscriberOptions);

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
            var categoryModel = new CreateIndexModel<SubscriberCategorySettings<ObjectId>>(categoryIndex, categoryOptions);

            IMongoCollection<SubscriberCategorySettings<ObjectId>> collection = Context.SubscriberCategorySettings;
            string subscriberName = collection.Indexes.CreateOne(subscriberModel);
            string categoryName = collection.Indexes.CreateOne(categoryModel);

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
            var topicModel = new CreateIndexModel<SubscriberTopicSettings<ObjectId>>(topicIndex, topicOptions);

            IndexKeysDefinition<SubscriberTopicSettings<ObjectId>> subscriberIndex = Builders<SubscriberTopicSettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);
            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };
            var subscriberModel = new CreateIndexModel<SubscriberTopicSettings<ObjectId>>(subscriberIndex, subscriberOptions);


            IMongoCollection<SubscriberTopicSettings<ObjectId>> collection = Context.SubscriberTopicSettings;
            string topicName = collection.Indexes.CreateOne(topicModel);
            string subscriberName = collection.Indexes.CreateOne(subscriberModel);

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
            var subscriberModel = new CreateIndexModel<SubscriberScheduleSettings<ObjectId>>(subscriberIndex, subscriberOptions);

            IMongoCollection<SubscriberScheduleSettings<ObjectId>> collection = Context.SubscriberReceivePeriods;
            string subscriberName = collection.Indexes.CreateOne(subscriberModel);
        }
        public virtual void CreateSignalEventIndex()
        {
            var failedAttemptsIndex = Builders<SignalEvent<ObjectId>>.IndexKeys
               .Ascending(p => p.CreateDateUtc)
               .Ascending(p => p.FailedAttempts);
            CreateIndexOptions failedAttemptsOptions = new CreateIndexOptions()
            {
                Name = "FailedAttempts",
                Unique = false
            };
            var failedAttemptsModel = new CreateIndexModel<SignalEvent<ObjectId>>(failedAttemptsIndex, failedAttemptsOptions);

            IMongoCollection<SignalEvent<ObjectId>> collection = Context.SignalEvents;
            string failedAttemptsName = collection.Indexes.CreateOne(failedAttemptsModel);
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
            var sendDateModel = new CreateIndexModel<SignalDispatch<ObjectId>>(sendDateIndex, sendDateOptions);

            var receiverIndex = Builders<SignalDispatch<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiverSubscriberId)
               .Ascending(p => p.SendDateUtc);
            CreateIndexOptions receiverOptions = new CreateIndexOptions()
            {
                Name = "ReceiverSubscriberId + SendDateUtc",
                Unique = false
            };
            var receiverModel = new CreateIndexModel<SignalDispatch<ObjectId>>(receiverIndex, receiverOptions);

            IMongoCollection<SignalDispatch<ObjectId>> collection = Context.SignalDispatches;
            string sendDateName = collection.Indexes.CreateOne(sendDateModel);
            string receiverName = collection.Indexes.CreateOne(receiverModel);

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
            var model = new CreateIndexModel<StoredNotification<ObjectId>>(subscriberIndex, sendDateOptions);

            IMongoCollection<StoredNotification<ObjectId>> collection = Context.StoredNotifications;
            string subscriberName = collection.Indexes.CreateOne(model);

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
            var model = new CreateIndexModel<SignalBounce<ObjectId>>(subscriberIndex, subscriberOptions);

            IMongoCollection<SignalBounce<ObjectId>> collection = Context.SignalBounces;
            string subscriberName = collection.Indexes.CreateOne(model);

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
            var model = new CreateIndexModel<EventSettings<ObjectId>>(subscriberIndex, subscriberOptions);

            IMongoCollection<EventSettings<ObjectId>> collection = Context.EventSettings;
            string subscriberName = collection.Indexes.CreateOne(model);

        }
        public virtual void CreateDispatchTemplateIndex()
        {
            //no extra indexes requied
        }
    }
}
