using MongoDB.Bson;
using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberCategorySettingsQueries<TCategory> :
        ISubscriberCategorySettingsQueries<TCategory, ObjectId>
        where TCategory : SubscriberCategorySettings<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSubscriberCategorySettingsQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }



        //methods
        public virtual Task Insert(List<TCategory> settings)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            return _collectionFactory
                .GetCollection<TCategory>()
                .InsertManyAsync(settings, options);
        }

        public virtual async Task<List<TCategory>> Select(List<ObjectId> subscriberIds, int categoryId)
        {
            var filter = Builders<TCategory>.Filter.Where(
                   p => subscriberIds.Contains(p.SubscriberId)
                   && p.CategoryId == categoryId);

            List<TCategory> list = await _collectionFactory
                .GetCollection<TCategory>()
                .Find(filter)
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public async Task<TotalResult<List<TCategory>>> Find(int pageIndex, int pageSize, bool descending)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            var filter = Builders<TCategory>.Filter.Where(x => true);

            Task<List<TCategory>> listTask = _collectionFactory
                .GetCollection<TCategory>()
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> totalCountTask = _collectionFactory
                .GetCollection<TCategory>()
                .EstimatedDocumentCountAsync();

            List<TCategory> list = await listTask;
            long totalCount = await totalCountTask;

            return new TotalResult<List<TCategory>>(list, totalCount);
        }

        public virtual async Task UpdateIsEnabled(List<TCategory> items)
        {
            var requests = new List<WriteModel<TCategory>>();

            foreach (TCategory item in items)
            {
                var filter = Builders<TCategory>.Filter.Where(
                    p => p.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

                var update = Builders<TCategory>.Update
                    .Set(p => p.IsEnabled, item.IsEnabled);

                requests.Add(new UpdateOneModel<TCategory>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<TCategory>()
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
        }

        public async Task UpsertIsEnabled(List<TCategory> items)
        {
            var requests = new List<WriteModel<TCategory>>();

            foreach (TCategory item in items)
            {
                var filter = Builders<TCategory>.Filter.Where(
                    p => p.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

                if (item.IsEnabled)
                {
                    var update = Builders<TCategory>.Update
                        .Set(p => p.IsEnabled, item.IsEnabled);
                    requests.Add(new UpdateOneModel<TCategory>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
                else
                {
                    requests.Add(new DeleteOneModel<TCategory>(filter));
                }
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<TCategory>()
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<TCategory>.Filter.Where(
                p => p.SubscriberId == subscriberId);

            DeleteResult response = await _collectionFactory
                .GetCollection<TCategory>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(TCategory item)
        {
            var filter = Builders<TCategory>.Filter.Where(
                    p => p.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

            DeleteResult response = await _collectionFactory
                .GetCollection<TCategory>()
                .DeleteOneAsync(filter)
                .ConfigureAwait(false);
        }



    }
}
