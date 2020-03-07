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

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberCategorySettingsQueries : ISubscriberCategorySettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberCategorySettingsQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<SubscriberCategorySettings<ObjectId>> settings)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.SubscriberCategorySettings.InsertManyAsync(settings, options);
        }

        public virtual async Task<List<SubscriberCategorySettings<ObjectId>>> Select(List<ObjectId> subscriberIds, int categoryId)
        {
            var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                   p => subscriberIds.Contains(p.SubscriberId)
                   && p.CategoryId == categoryId);

            List<SubscriberCategorySettings<ObjectId>> list = await _context.SubscriberCategorySettings.Find(filter).ToListAsync();

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public async Task<TotalResult<List<SubscriberCategorySettings<ObjectId>>>> Find(int pageIndex, int pageSize, bool descending)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(x => true);

            Task<List<SubscriberCategorySettings<ObjectId>>> listTask = _context
                .SubscriberCategorySettings.Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> totalCountTask = _context.SubscriberCategorySettings.EstimatedDocumentCountAsync();

            List<SubscriberCategorySettings<ObjectId>> list = await listTask;
            long totalCount = await totalCountTask;

            return new TotalResult<List<SubscriberCategorySettings<ObjectId>>>(list, totalCount);
        }

        public virtual async Task UpdateIsEnabled(List<SubscriberCategorySettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<SubscriberCategorySettings<ObjectId>>>();

            foreach (SubscriberCategorySettings<ObjectId> item in items)
            {
                var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

                var update = Builders<SubscriberCategorySettings<ObjectId>>.Update
                    .Set(p => p.IsEnabled, item.IsEnabled);

                requests.Add(new UpdateOneModel<SubscriberCategorySettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.SubscriberCategorySettings
                .BulkWriteAsync(requests, options);
        }

        public async Task UpsertIsEnabled(List<SubscriberCategorySettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<SubscriberCategorySettings<ObjectId>>>();

            foreach (SubscriberCategorySettings<ObjectId> item in items)
            {
                var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

                if (item.IsEnabled)
                {
                    var update = Builders<SubscriberCategorySettings<ObjectId>>.Update
                        .Set(p => p.IsEnabled, item.IsEnabled);
                    requests.Add(new UpdateOneModel<SubscriberCategorySettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
                else
                {
                    requests.Add(new DeleteOneModel<SubscriberCategorySettings<ObjectId>>(filter));
                }
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.SubscriberCategorySettings
                .BulkWriteAsync(requests, options);
        }

        public virtual async Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                p => p.SubscriberId == subscriberId);

            DeleteResult response = await _context.SubscriberCategorySettings.DeleteManyAsync(filter);
        }

        public virtual async Task Delete(SubscriberCategorySettings<ObjectId> item)
        {
            var filter = Builders<SubscriberCategorySettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberCategorySettingsId == item.SubscriberCategorySettingsId);

            DeleteResult response = await _context.SubscriberCategorySettings.DeleteOneAsync(filter);
        }



    }
}
