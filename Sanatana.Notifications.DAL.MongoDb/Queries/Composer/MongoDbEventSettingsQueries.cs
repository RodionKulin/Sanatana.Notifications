using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbEventSettingsQueries : IEventSettingsQueries<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbEventSettingsQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }



        //methods
        public virtual Task Insert(List<EventSettings<ObjectId>> items)
        {
            foreach (EventSettings<ObjectId> item in items)
            {
                item.EventSettingsId = ObjectId.GenerateNewId();
            }

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            return _collectionFactory.GetCollection<EventSettings<ObjectId>>()
                .InsertManyAsync(items, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<EventSettings<ObjectId>>>> Select(int pageIndex, int pageSize)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(p => true);

            IMongoCollection<EventSettings<ObjectId>> eventSettings = _collectionFactory.GetCollection<EventSettings<ObjectId>>();

            Task<List<EventSettings<ObjectId>>> listTask = eventSettings
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> countTask = eventSettings.EstimatedDocumentCountAsync();

            long count = await countTask.ConfigureAwait(false);
            List<EventSettings<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<EventSettings<ObjectId>>>(list, count);
        }

        public virtual async Task<EventSettings<ObjectId>> Select(ObjectId eventSettingsId)
        {
            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                p => p.EventSettingsId == eventSettingsId);

            EventSettings<ObjectId> item = await _collectionFactory
                .GetCollection<EventSettings<ObjectId>>()
                .Find(filter)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return item;
        }

        public virtual async Task<List<EventSettings<ObjectId>>> SelectByKey(int eventKey)
        {
            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                p => p.EventKey == eventKey);

            List<EventSettings<ObjectId>> list = await _collectionFactory
                .GetCollection<EventSettings<ObjectId>>()
                .Find(filter)
                .ToListAsync()
                .ConfigureAwait(false);

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

            BulkWriteResult response = await _collectionFactory
                .GetCollection<EventSettings<ObjectId>>()
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(List<EventSettings<ObjectId>> items)
        {
            List<ObjectId> Ids = items.Select(p => p.EventSettingsId).ToList();

            var filter = Builders<EventSettings<ObjectId>>.Filter.Where(
                p => Ids.Contains(p.EventSettingsId));

            DeleteResult response = await _collectionFactory
                .GetCollection<EventSettings<ObjectId>>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
