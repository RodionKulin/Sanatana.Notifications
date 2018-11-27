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

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbStoredNotificationQueries : IStoredNotificationQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        protected SenderMongoDbContext _context;

        //init
        public MongoDbStoredNotificationQueries(MongoDbConnectionSettings connectionSettings)
        {
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }


        //methods
        public async Task Insert(List<StoredNotification<ObjectId>> items)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = true
            };

            await _context.StoredNotifications.InsertManyAsync(items, options).ConfigureAwait(false);
        }

        public async Task<TotalResult<List<StoredNotification<ObjectId>>>> Select(
            List<ObjectId> subscriberIds, int page, int pageSize, bool descending)
        {
            int skip = (page - 1) * pageSize;
            
            var filter = Builders<StoredNotification<ObjectId>>.Filter.Where(
                    p => subscriberIds.Contains(p.SubscriberId));

            var options = new FindOptions()
            {
                AllowPartialResults = false
            };

            IFindFluent<StoredNotification<ObjectId>, StoredNotification<ObjectId>> fluent 
                = _context.StoredNotifications.Find(filter, options);

            if (descending)
            {
                fluent = fluent.SortByDescending(x => x.CreateDateUtc);
            }
            else
            {
                fluent = fluent.SortBy(x => x.CreateDateUtc);
            }

            Task<long> countTask = _context.StoredNotifications.CountAsync(filter, new CountOptions
            {
            });
            Task<List<StoredNotification<ObjectId>>> listTask = fluent
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            long count = await countTask.ConfigureAwait(false);
            List<StoredNotification<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<StoredNotification<ObjectId>>>(list, count);
        }

        public async Task Update(List<StoredNotification<ObjectId>> items)
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

            BulkWriteResult response = await _context.StoredNotifications
                .BulkWriteAsync(requests, options);
        }

        public async Task Delete(List<StoredNotification<ObjectId>> items)
        {
            List<ObjectId> ids = items
                .Select(p => p.StoredNotificationId).ToList();

            var filter = Builders<StoredNotification<ObjectId>>.Filter.Where(
                p => ids.Contains(p.StoredNotificationId));

            DeleteResult response = await _context.StoredNotifications
                .DeleteManyAsync(filter).ConfigureAwait(false);
        }

    }
}
