using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbDispatchTemplateQueries : IDispatchTemplateQueries<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbDispatchTemplateQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }


        //methods
        public Task Insert(List<DispatchTemplate<ObjectId>> items)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = true
            };

            return _collectionFactory
                .GetCollection<DispatchTemplate<ObjectId>>()
                .InsertManyAsync(items, options);
        }

        /// <summary>
        /// Select DispatchTemplate items page
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<TotalResult<List<DispatchTemplate<ObjectId>>>> SelectPage(int pageIndex, int pageSize)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);
            var filter = Builders<DispatchTemplate<ObjectId>>.Filter.Where(p => true);

            var options = new FindOptions()
            {
                AllowPartialResults = false
            };

            IMongoCollection<DispatchTemplate<ObjectId>> dispatchTemplates = 
                _collectionFactory.GetCollection<DispatchTemplate<ObjectId>>();

            Task<long> countTask = dispatchTemplates.EstimatedDocumentCountAsync();
            Task<List<DispatchTemplate<ObjectId>>> listTask = dispatchTemplates.Find(filter, options)
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

            DispatchTemplate<ObjectId> dispatchTemplate = await _collectionFactory
                .GetCollection<DispatchTemplate<ObjectId>>()
                .Find(filter, options)
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

            List<DispatchTemplate<ObjectId>> list = await _collectionFactory
                .GetCollection<DispatchTemplate<ObjectId>>()
                .Find(filter, options)
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

            BulkWriteResult response = await _collectionFactory
                .GetCollection<DispatchTemplate<ObjectId>>()
                .BulkWriteAsync(requests, options);
        }

        public async Task Delete(List<DispatchTemplate<ObjectId>> items)
        {
            List<ObjectId> ids = items.Select(p => p.DispatchTemplateId).ToList();

            var filter = Builders<DispatchTemplate<ObjectId>>.Filter.Where(
                p => ids.Contains(p.DispatchTemplateId));

            DeleteResult response = await _collectionFactory
                .GetCollection<DispatchTemplate<ObjectId>>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
