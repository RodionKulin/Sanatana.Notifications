using MongoDB.Bson;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbStoredNotificationQueries : IStoredNotificationQueries<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbStoredNotificationQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }


        //methods
        public virtual async Task Insert(List<StoredNotification<ObjectId>> items)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = true
            };

            await _collectionFactory
                .GetCollection<StoredNotification<ObjectId>>()
                .InsertManyAsync(items, options)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberIds"></param>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<StoredNotification<ObjectId>>>> Select(
            List<ObjectId> subscriberIds, int pageIndex, int pageSize, bool descending)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            
            var filter = Builders<StoredNotification<ObjectId>>.Filter.Where(
                    p => subscriberIds.Contains(p.SubscriberId));

            var options = new FindOptions()
            {
                AllowPartialResults = false
            };

            IFindFluent<StoredNotification<ObjectId>, StoredNotification<ObjectId>> fluent = _collectionFactory
                .GetCollection<StoredNotification<ObjectId>>()
                .Find(filter, options);

            if (descending)
            {
                fluent = fluent.SortByDescending(x => x.CreateDateUtc);
            }
            else
            {
                fluent = fluent.SortBy(x => x.CreateDateUtc);
            }

            Task<long> countTask = _collectionFactory
                .GetCollection<StoredNotification<ObjectId>>()
                .CountDocumentsAsync(filter);
            Task<List<StoredNotification<ObjectId>>> listTask = fluent
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            long count = await countTask.ConfigureAwait(false);
            List<StoredNotification<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<StoredNotification<ObjectId>>>(list, count);
        }

        public virtual async Task Update(List<StoredNotification<ObjectId>> items)
        {
            var requests = new List<WriteModel<StoredNotification<ObjectId>>>();

            foreach (StoredNotification<ObjectId> item in items)
            {
                var filter = Builders<StoredNotification<ObjectId>>.Filter.Where(
                    p => p.StoredNotificationId == item.StoredNotificationId);

                var update = Builders<StoredNotification<ObjectId>>.Update
                    .SetAllMappedMembers(item);

                requests.Add(new UpdateOneModel<StoredNotification<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<StoredNotification<ObjectId>>()
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(List<StoredNotification<ObjectId>> items)
        {
            List<ObjectId> ids = items.Select(p => p.StoredNotificationId).ToList();

            var filter = Builders<StoredNotification<ObjectId>>.Filter.Where(
                p => ids.Contains(p.StoredNotificationId));

            DeleteResult response = await _collectionFactory
                .GetCollection<StoredNotification<ObjectId>>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
