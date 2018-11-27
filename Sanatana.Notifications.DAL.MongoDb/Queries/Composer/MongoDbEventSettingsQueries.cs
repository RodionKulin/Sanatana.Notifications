using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbEventSettingsQueries : IEventSettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;

        protected SenderMongoDbContext _context;


        //init
        public MongoDbEventSettingsQueries(MongoDbConnectionSettings connectionSettings)
        {
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<EventSettings<ObjectId>> items)
        {
            foreach (EventSettings<ObjectId> item in items)
            {
                item.EventSettingsId = ObjectId.GenerateNewId();
            }

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.EventSettings.InsertManyAsync(items, options).ConfigureAwait(false);
        }

        public virtual async Task<TotalResult<List<EventSettings<ObjectId>>>> Select(int page, int pageSize)
        {
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);
            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(p => true);

            Task<List<EventSettings<ObjectId>>> listTask = _context.EventSettings
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> countTask = _context.EventSettings.CountAsync(filter);

            long count = await countTask.ConfigureAwait(false);
            List<EventSettings<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<EventSettings<ObjectId>>>(list, count);
        }

        public virtual async Task<EventSettings<ObjectId>> Select(ObjectId eventSettingsId)
        {
            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                p => p.EventSettingsId == eventSettingsId);

            EventSettings<ObjectId> item = await _context.EventSettings.Find(filter)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            return item;
        }

        public virtual async Task<List<EventSettings<ObjectId>>> Select(int category)
        {
            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                p => p.CategoryId == category);

            List<EventSettings<ObjectId>> list = await _context.EventSettings.Find(filter)
                .ToListAsync().ConfigureAwait(false);

            return list;
        }

        public virtual async Task Update(List<EventSettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<EventSettings<ObjectId>>>();

            foreach (EventSettings<ObjectId> item in items)
            {
                var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                    p => p.EventSettingsId == item.EventSettingsId);

                var update = Builders<EventSettings<ObjectId>>.Update
                    .Set(p => p.Subscription, item.Subscription)
                    .Set(p => p.Updates, item.Updates)
                    .Set(p => p.Templates, item.Templates);

                requests.Add(new UpdateOneModel<EventSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.EventSettings
                .BulkWriteAsync(requests, options).ConfigureAwait(false);
        }

        public virtual async Task Delete(List<EventSettings<ObjectId>> items)
        {
            List<ObjectId> Ids = items.Select(p => p.EventSettingsId).ToList();

            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                p => Ids.Contains(p.EventSettingsId));

            DeleteResult response = await _context.EventSettings.DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
