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
    public class MongoDbComposerQueries : IComposerSettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbComposerQueries(MongoDbConnectionSettings connectionSettings)
        {
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<ComposerSettings<ObjectId>> items)
        {
            foreach (ComposerSettings<ObjectId> item in items)
            {
                item.ComposerSettingsId = ObjectId.GenerateNewId();
            }

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.ComposerSettings.InsertManyAsync(items, options).ConfigureAwait(false);
        }

        public virtual async Task<TotalResult<List<ComposerSettings<ObjectId>>>> Select(int page, int pageSize)
        {
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);
            var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(p => true);

            Task<List<ComposerSettings<ObjectId>>> listTask = _context.ComposerSettings
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> countTask = _context.ComposerSettings.CountAsync(filter);

            long count = await countTask.ConfigureAwait(false);
            List<ComposerSettings<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<ComposerSettings<ObjectId>>>(list, count);
        }

        public virtual async Task<ComposerSettings<ObjectId>> Select(ObjectId composerSettingsId)
        {
            var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                p => p.ComposerSettingsId == composerSettingsId);

            ComposerSettings<ObjectId> item = await _context.ComposerSettings.Find(filter)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            return item;
        }

        public virtual async Task<List<ComposerSettings<ObjectId>>> Select(int category)
        {
            var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                p => p.CategoryId == category);

            List<ComposerSettings<ObjectId>> list = await _context.ComposerSettings.Find(filter)
                .ToListAsync().ConfigureAwait(false);

            return list;
        }

        public virtual async Task Update(List<ComposerSettings<ObjectId>> items)
        {
            var requests = new List<WriteModel<ComposerSettings<ObjectId>>>();

            foreach (ComposerSettings<ObjectId> item in items)
            {
                var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                    p => p.ComposerSettingsId == item.ComposerSettingsId);

                var update = Builders<ComposerSettings<ObjectId>>.Update
                    .Set(p => p.Subscription, item.Subscription)
                    .Set(p => p.Updates, item.Updates)
                    .Set(p => p.Templates, item.Templates);

                requests.Add(new UpdateOneModel<ComposerSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.ComposerSettings
                .BulkWriteAsync(requests, options).ConfigureAwait(false);
        }

        public virtual async Task Delete(List<ComposerSettings<ObjectId>> items)
        {
            List<ObjectId> Ids = items.Select(p => p.ComposerSettingsId).ToList();

            var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                p => Ids.Contains(p.ComposerSettingsId));

            DeleteResult response = await _context.ComposerSettings.DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
