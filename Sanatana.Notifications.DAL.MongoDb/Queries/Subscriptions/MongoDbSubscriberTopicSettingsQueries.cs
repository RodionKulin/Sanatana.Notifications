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
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberTopicSettingsQueries<TTopic> : ISubscriberTopicSettingsQueries<TTopic, ObjectId>
        where TTopic : SubscriberTopicSettings<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSubscriberTopicSettingsQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }



        //methods
        public virtual async Task Insert(List<TTopic> settings)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _collectionFactory
                .GetCollection<TTopic>()
                .InsertManyAsync(settings, options)
                .ConfigureAwait(false);
        }



        public virtual async Task<TTopic> Select(
            ObjectId subscriberId, int categoryId, string topicId)
        {
            var filter = Builders<TTopic>.Filter.Where(
                p => p.SubscriberId == subscriberId
                && p.CategoryId == categoryId
                && p.TopicId == topicId);

            //string explain = _collectionFactory.GetCollection<TTopic>().ExplainFind<TTopic, TTopic>
            //    (ExplainVerbosity.QueryPlanner, filter).Data.ToJsonIntended();

            TTopic item = await _collectionFactory
                .GetCollection<TTopic>()
                .Find(filter)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="subscriberIds"></param>
        /// <param name="deliveryTypeIds"></param>
        /// <param name="categoryIds"></param>
        /// <param name="topicIds"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<TTopic>>> SelectPage(int pageIndex, int pageSize
            , List<ObjectId> subscriberIds = null, List<int> deliveryTypeIds = null, List<int> categoryIds = null
            , List<string> topicIds = null)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);

            var filter = Builders<TTopic>.Filter.Where(p => p.IsDeleted == false);

            if (subscriberIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => subscriberIds.Contains(p.SubscriberId)));
            }
            if (deliveryTypeIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => deliveryTypeIds.Contains(p.DeliveryType)));
            }
            if (categoryIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => categoryIds.Contains(p.CategoryId)));
            }
            if (topicIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => topicIds.Contains(p.TopicId)));
            }

            Task<List<TTopic>> listTask = _collectionFactory.GetCollection<TTopic>().Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            Task<long> countTask = _collectionFactory.GetCollection<TTopic>().CountDocumentsAsync(filter);

            List<TTopic> list = await listTask.ConfigureAwait(false);
            long total = await countTask.ConfigureAwait(false);

            return new TotalResult<List<TTopic>>(list, total);
        }



        public virtual async Task UpdateIsDeleted(TTopic settings)
        {
            var filter = Builders<TTopic>.Filter.Where(
                     p => p.SubscriberId == settings.SubscriberId
                     && p.CategoryId == settings.CategoryId
                     && p.TopicId == settings.TopicId);

            var update = Builders<TTopic>.Update
                .Set(p => p.IsDeleted, settings.IsDeleted);

            UpdateResult response = await _collectionFactory
                .GetCollection<TTopic>()
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task Upsert(TTopic settings, bool updateExisting)
        {
            var filter = Builders<TTopic>.Filter.Where(
                    p => p.SubscriberId == settings.SubscriberId
                    && p.CategoryId == settings.CategoryId
                    && p.TopicId == settings.TopicId);

            var update = Builders<TTopic>.Update
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

            UpdateResult response = await _collectionFactory
                .GetCollection<TTopic>()
                .UpdateOneAsync(filter, update, options)
                .ConfigureAwait(false);
        }



        public virtual async Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<TTopic>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            DeleteResult response = await _collectionFactory
                .GetCollection<TTopic>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(List<ObjectId> subscriberIds = null, List<int> deliveryTypeIds = null
            , List<int> categoryIds = null, List<string> topicIds = null)
        {
            var filter = Builders<TTopic>.Filter.Where(p => true);

            if (subscriberIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => subscriberIds.Contains(p.SubscriberId)));
            }
            if (deliveryTypeIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => deliveryTypeIds.Contains(p.DeliveryType)));
            }
            if (categoryIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => categoryIds.Contains(p.CategoryId)));
            }
            if (topicIds != null)
            {
                filter = Builders<TTopic>.Filter.And(filter,
                    Builders<TTopic>.Filter.Where(p => topicIds.Contains(p.TopicId)));
            }
            
            DeleteResult response = await _collectionFactory
                .GetCollection<TTopic>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }



        public virtual async Task UpdateIsEnabled(List<TTopic> items)
        {
            var requests = new List<WriteModel<TTopic>>();

            foreach (TTopic item in items)
            {
                var filter = Builders<TTopic>.Filter.Where(
                    p => p.SubscriberTopicSettingsId == item.SubscriberTopicSettingsId);

                var update = Builders<TTopic>.Update
                    .Set(p => p.IsEnabled, item.IsEnabled);

                requests.Add(new UpdateOneModel<TTopic>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory.GetCollection<TTopic>()
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
        }

        public virtual async Task UpsertIsEnabled(List<TTopic> items)
        {
            var requests = new List<WriteModel<TTopic>>();

            foreach (TTopic item in items)
            {
                var filter = Builders<TTopic>.Filter.Where(
                    p => p.SubscriberTopicSettingsId == item.SubscriberTopicSettingsId);

                if (item.IsEnabled)
                {
                    var update = Builders<TTopic>.Update
                        .Set(p => p.IsEnabled, item.IsEnabled);
                    requests.Add(new UpdateOneModel<TTopic>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
                else
                {
                    requests.Add(new DeleteOneModel<TTopic>(filter));
                }
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<TTopic>()
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
        }
    }
}
