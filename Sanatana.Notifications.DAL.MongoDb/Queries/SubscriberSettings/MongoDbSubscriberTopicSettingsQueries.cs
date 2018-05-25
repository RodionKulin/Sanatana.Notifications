using MongoDB.Bson;
using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sanatana.MongoDb;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbSubscriberTopicSettingsQueries : ISubscriberTopicSettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberTopicSettingsQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<SubscriberTopicSettings<ObjectId>> settings)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.SubscriberTopicSettings.InsertManyAsync(settings, options);
        }



        public virtual async Task<SubscriberTopicSettings<ObjectId>> Select(
            ObjectId subscriberId, int categoryId, string topicId)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                p => p.SubscriberId == subscriberId
                && p.CategoryId == categoryId
                && p.TopicId == topicId);

            //string explain = _context.SubscriberTopicSettings.ExplainFind<SubscriberTopicSettings<ObjectId>, SubscriberTopicSettings<ObjectId>>
            //    (ExplainVerbosity.QueryPlanner, filter).Data.ToJsonIntended();

            SubscriberTopicSettings<ObjectId> item = await _context.SubscriberTopicSettings.Find(filter).FirstOrDefaultAsync();
            
            return item;
        }

        public virtual async Task<TotalResult<List<SubscriberTopicSettings<ObjectId>>>> SelectPage(int page, int pageSize
            , List<ObjectId> subscriberIds = null, List<int> deliveryTypeIds = null, List<int> categoryIds = null
            , List<string> topicIds = null)
        {
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);

            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => p.IsDeleted == false);

            if (subscriberIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => subscriberIds.Contains(p.SubscriberId)));
            }
            if (deliveryTypeIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => deliveryTypeIds.Contains(p.DeliveryType)));
            }
            if (categoryIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => categoryIds.Contains(p.CategoryId)));
            }
            if (topicIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => topicIds.Contains(p.TopicId)));
            }

            Task<List<SubscriberTopicSettings<ObjectId>>> listTask = _context.SubscriberTopicSettings.Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            Task<long> countTask = _context.SubscriberTopicSettings.CountAsync(filter);

            List<SubscriberTopicSettings<ObjectId>> list = await listTask;
            long total = await countTask;
            
            return new TotalResult<List<SubscriberTopicSettings<ObjectId>>>(list, total);
        }



        public virtual async Task UpdateIsDeleted(SubscriberTopicSettings<ObjectId> settings)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                     p => p.SubscriberId == settings.SubscriberId
                     && p.CategoryId == settings.CategoryId
                     && p.TopicId == settings.TopicId);

            var update = Builders<SubscriberTopicSettings<ObjectId>>.Update
                .Set(p => p.IsDeleted, settings.IsDeleted);

            UpdateResult response = await _context.SubscriberTopicSettings.UpdateOneAsync(filter, update);
        }

        public virtual async Task Upsert(SubscriberTopicSettings<ObjectId> settings, bool updateExisting)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == settings.SubscriberId
                    && p.CategoryId == settings.CategoryId
                    && p.TopicId == settings.TopicId);

            var update = Builders<SubscriberTopicSettings<ObjectId>>.Update
                .Combine()
                .SetOnInsertAllMappedMembers(settings);

            if (updateExisting)
            {
                update = update.Set(p => p.IsEnabled, settings.IsEnabled)
                    .Set(p => p.IsDeleted, settings.IsDeleted);
            }

            var options = new UpdateOptions()
            {
                IsUpsert = true
            };

            UpdateResult response = await _context.SubscriberTopicSettings.UpdateOneAsync(filter, update, options);
        }



        public virtual async Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            DeleteResult response = await _context.SubscriberTopicSettings.DeleteManyAsync(filter);
        }

        public virtual async Task Delete(List<ObjectId> subscriberIds = null, List<int> deliveryTypeIds = null
            , List<int> categoryIds = null, List<string> topicIds = null)
        {
            var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => true);

            if (subscriberIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => subscriberIds.Contains(p.SubscriberId)));
            }
            if (deliveryTypeIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => deliveryTypeIds.Contains(p.DeliveryType)));
            }
            if (categoryIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => categoryIds.Contains(p.CategoryId)));
            }
            if (topicIds != null)
            {
                filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.And(filter,
                    Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(p => topicIds.Contains(p.TopicId)));
            }
            
            DeleteResult response = await _context.SubscriberTopicSettings.DeleteManyAsync(filter);
        }



        public virtual async Task UpdateIsEnabled(List<SubscriberTopicSettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<SubscriberTopicSettings<ObjectId>>>();

            foreach (SubscriberTopicSettings<ObjectId> item in items)
            {
                var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberTopicSettingsId == item.SubscriberTopicSettingsId);

                var update = Builders<SubscriberTopicSettings<ObjectId>>.Update
                    .Set(p => p.IsEnabled, item.IsEnabled);

                requests.Add(new UpdateOneModel<SubscriberTopicSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.SubscriberTopicSettings
                .BulkWriteAsync(requests, options);
        }

        public virtual async Task UpsertIsEnabled(List<SubscriberTopicSettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<SubscriberTopicSettings<ObjectId>>>();

            foreach (SubscriberTopicSettings<ObjectId> item in items)
            {
                var filter = Builders<SubscriberTopicSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberTopicSettingsId == item.SubscriberTopicSettingsId);

                if (item.IsEnabled)
                {
                    var update = Builders<SubscriberTopicSettings<ObjectId>>.Update
                        .Set(p => p.IsEnabled, item.IsEnabled);
                    requests.Add(new UpdateOneModel<SubscriberTopicSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
                else
                {
                    requests.Add(new DeleteOneModel<SubscriberTopicSettings<ObjectId>>(filter));
                }
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.SubscriberTopicSettings
                .BulkWriteAsync(requests, options);
        }
    }
}
