using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbDispatchTemplateQueries : IDispatchTemplateQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        protected SenderMongoDbContext _context;


        //init
        public MongoDbDispatchTemplateQueries(MongoDbConnectionSettings connectionSettings)
        {
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }


        //methods
        public async Task Insert(List<DispatchTemplate<ObjectId>> items)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = true
            };

            await _context.DispatchTemplates.InsertManyAsync(items, options).ConfigureAwait(false);
        }

        public async Task<TotalResult<List<DispatchTemplate<ObjectId>>>> Select(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;
            var filter = Builders<DispatchTemplate<ObjectId>>.Filter.Where(p => true);

            var options = new FindOptions()
            {
                AllowPartialResults = false
            };

            Task<long> countTask = _context.DispatchTemplates.CountAsync(filter, new CountOptions
            {
            });
            Task<List<DispatchTemplate<ObjectId>>> listTask = _context.DispatchTemplates.Find(filter, options)
                .SortByDescending(x => x.DispatchTemplateId)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            long count = await countTask.ConfigureAwait(false);
            List<DispatchTemplate<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<DispatchTemplate<ObjectId>>>(list, count);
        }

        public async Task<DispatchTemplate<ObjectId>> Select(ObjectId dispatchTemplatesId)
        {
            var filter = Builders<DispatchTemplate<ObjectId>>.Filter
                .Where(p => p.DispatchTemplateId == dispatchTemplatesId);

            var options = new FindOptions()
            {
                AllowPartialResults = false
            };

            DispatchTemplate<ObjectId> dispatchTemplate = await _context.DispatchTemplates.Find(filter, options)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return dispatchTemplate;
        }

        public async Task<List<DispatchTemplate<ObjectId>>> SelectForEventSettings(ObjectId eventSettingsId)
        {
            var filter = Builders<DispatchTemplate<ObjectId>>.Filter
                .Where(p => p.EventSettingsId == eventSettingsId);

            var options = new FindOptions()
            {
                AllowPartialResults = false
            };

            List<DispatchTemplate<ObjectId>> list = await _context.DispatchTemplates.Find(filter, options)
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        public async Task Update(List<DispatchTemplate<ObjectId>> items)
        {
            var requests = new List<WriteModel<DispatchTemplate<ObjectId>>>();

            foreach (DispatchTemplate<ObjectId> item in items)
            {
                var filter = Builders<DispatchTemplate<ObjectId>>.Filter.Where(
                    p => p.DispatchTemplateId == item.DispatchTemplateId);

                var update = Builders<DispatchTemplate<ObjectId>>.Update
                    .SetAllMappedMembers(item);

                requests.Add(new UpdateOneModel<DispatchTemplate<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.DispatchTemplates
                .BulkWriteAsync(requests, options);
        }

        public async Task Delete(List<DispatchTemplate<ObjectId>> items)
        {
            List<ObjectId> ids = items
                .Select(p => p.DispatchTemplateId).ToList();

            var filter = Builders<StoredNotification<ObjectId>>.Filter.Where(
                p => ids.Contains(p.StoredNotificationId));

            DeleteResult response = await _context.StoredNotifications
                .DeleteManyAsync(filter).ConfigureAwait(false);
        }

    }
}
