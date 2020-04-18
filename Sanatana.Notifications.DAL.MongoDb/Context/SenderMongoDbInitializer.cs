using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.MongoDb.Entities;

namespace Sanatana.Notifications.DAL.MongoDb.Context
{
    public class SenderMongoDbInitializer<TDeliveryType, TCategory, TTopic>
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>
        where TCategory : SubscriberCategorySettings<ObjectId>
        where TTopic : SubscriberTopicSettings<ObjectId>
    {
        //properties
        protected SenderMongoDbContext<TDeliveryType, TCategory, TTopic> _context;


        //init
        public SenderMongoDbInitializer(SenderMongoDbContext<TDeliveryType, TCategory, TTopic> context)
        {
            _context = context;
        }


        //methods
        public virtual void CreateAllIndexes(TimeSpan? historyExpirationTime)
        {
            CreateSubscriberDeliveryTypeSettingsIndex();
            CreateSubscriberCategorySettingsIndex();
            CreateSubscriberTopicSettingsIndex();
            CreateSubscriberReceivePeriodsIndex();
            CreateSignalEventIndex();
            CreateSignalDispatchIndex();
            CreateSignalDispatchHistoryIndex(historyExpirationTime);
            CreateStoredNotificationIndex();
            CreateSignalBounceIndex();
            CreateEventSettingsIndex();
            CreateDispatchTemplateIndex();
        }
        public virtual void DropAllIndexes()
        {
            _context.GetCollection<TDeliveryType>().Indexes.DropAll();
            _context.SubscriberCategorySettings.Indexes.DropAll();
            _context.SubscriberTopicSettings.Indexes.DropAll();
            _context.SubscriberScheduleSettings.Indexes.DropAll();
            _context.SignalEvents.Indexes.DropAll();
            _context.SignalDispatches.Indexes.DropAll();
            _context.StoredNotifications.Indexes.DropAll();
            _context.SignalBounces.Indexes.DropAll();
            _context.EventSettings.Indexes.DropAll();
            _context.DispatchTemplates.Indexes.DropAll();
        }


        public virtual void CreateSubscriberDeliveryTypeSettingsIndex()
        {
            IndexKeysDefinition<TDeliveryType> subscriberIndex = Builders<TDeliveryType>.IndexKeys
                .Ascending(p => p.SubscriberId);
            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };
            var subscriberModel = new CreateIndexModel<TDeliveryType>(subscriberIndex, subscriberOptions);

            IndexKeysDefinition<TDeliveryType> addressIndex = 
                Builders<TDeliveryType>.IndexKeys
                .Ascending(p => p.Address);
            CreateIndexOptions addressOptions = new CreateIndexOptions()
            {
                Name = "Address",
                Unique = false
            };
            var addressModel = new CreateIndexModel<TDeliveryType>(addressIndex, addressOptions);

            IMongoCollection<TDeliveryType> collection = _context.GetCollection<TDeliveryType>();
            string subscriberName = collection.Indexes.CreateOne(subscriberModel);
            string addressName = collection.Indexes.CreateOne(addressModel);
        }
        public virtual void CreateSubscriberCategorySettingsIndex()
        {
            IndexKeysDefinition<SubscriberCategorySettings<ObjectId>> subscriberIndex = Builders<SubscriberCategorySettings<ObjectId>>.IndexKeys
                .Ascending(p => p.SubscriberId);
            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "SubscriberId",
                Unique = false
            };
            var subscriberModel = new CreateIndexModel<SubscriberCategorySettings<ObjectId>>(subscriberIndex, subscriberOptions);

            var categoryIndex = Builders<SubscriberCategorySettings<ObjectId>>.IndexKeys
                .Ascending(p => p.CategoryId);
            CreateIndexOptions categoryOptions = new CreateIndexOptions()
            {
                Name = "CategoryId",
                Unique = false
            };
            var categoryModel = new CreateIndexModel<SubscriberCategorySettings<ObjectId>>(categoryIndex, categoryOptions);

            IMongoCollection<SubscriberCategorySettings<ObjectId>> collection = _context.SubscriberCategorySettings;
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


            IMongoCollection<SubscriberTopicSettings<ObjectId>> collection = _context.SubscriberTopicSettings;
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

            IMongoCollection<SubscriberScheduleSettings<ObjectId>> collection = _context.SubscriberScheduleSettings;
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

            IMongoCollection<SignalEvent<ObjectId>> collection = _context.SignalEvents;
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
            
            var lockIndex = Builders<SignalDispatch<ObjectId>>.IndexKeys
               .Ascending(p => p.LockedBy)
               .Ascending(p => p.LockedDateUtc);
            var lockOptions = new CreateIndexOptions<SignalDispatch<ObjectId>>()
            {
                Name = "LockedBy + LockedDateUtc",
                Unique = false,
                PartialFilterExpression = Builders<SignalDispatch<ObjectId>>.Filter.Ne(i => i.LockedBy, null)
            };
            var lockModel = new CreateIndexModel<SignalDispatch<ObjectId>>(lockIndex, lockOptions);

            IMongoCollection <SignalDispatch<ObjectId>> collection = _context.SignalDispatches;
            string sendDateName = collection.Indexes.CreateOne(sendDateModel);
            string receiverName = collection.Indexes.CreateOne(receiverModel);
            string lockName = collection.Indexes.CreateOne(lockModel);
        }
        public virtual void CreateSignalDispatchHistoryIndex(TimeSpan? historyExpirationTime)
        {
            var receiverIndex = Builders<SignalDispatch<ObjectId>>.IndexKeys
               .Ascending(p => p.ReceiverSubscriberId)
               .Ascending(p => p.SendDateUtc);
            CreateIndexOptions receiverOptions = new CreateIndexOptions()
            {
                Name = "ReceiverSubscriberId + SendDateUtc",
                Unique = false
            };
            var receiverModel = new CreateIndexModel<SignalDispatch<ObjectId>>(receiverIndex, receiverOptions);

            var ttlIndex = Builders<SignalDispatch<ObjectId>>.IndexKeys
               .Ascending(p => p.CreateDateUtc);
            CreateIndexOptions ttlOptions = new CreateIndexOptions()
            {
                Name = "CreateDateUtc TTL",
                Unique = false,
                ExpireAfter = historyExpirationTime
            };
            var ttlModel = new CreateIndexModel<SignalDispatch<ObjectId>>(ttlIndex, ttlOptions);

            IMongoCollection<SignalDispatch<ObjectId>> collection = _context.SignalDispatchesHistory;
            string receiverName = collection.Indexes.CreateOne(receiverModel);
            if(historyExpirationTime != null)
            {
                string ttlName = collection.Indexes.CreateOne(ttlModel);
            }
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

            IMongoCollection<StoredNotification<ObjectId>> collection = _context.StoredNotifications;
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

            IMongoCollection<SignalBounce<ObjectId>> collection = _context.SignalBounces;
            string subscriberName = collection.Indexes.CreateOne(model);

        }
        public virtual void CreateEventSettingsIndex()
        {
            IndexKeysDefinition<EventSettings<ObjectId>> subscriberIndex = Builders<EventSettings<ObjectId>>.IndexKeys
               .Ascending(p => p.EventKey);
            CreateIndexOptions subscriberOptions = new CreateIndexOptions()
            {
                Name = "EventKey",
                Unique = false
            };
            var model = new CreateIndexModel<EventSettings<ObjectId>>(subscriberIndex, subscriberOptions);

            IMongoCollection<EventSettings<ObjectId>> collection = _context.EventSettings;
            string subscriberName = collection.Indexes.CreateOne(model);

        }
        public virtual void CreateDispatchTemplateIndex()
        {
            //no extra indexes requied
        }
    }
}
