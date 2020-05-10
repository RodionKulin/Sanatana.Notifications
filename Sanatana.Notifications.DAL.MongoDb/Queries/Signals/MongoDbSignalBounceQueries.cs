using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSignalBounceQueries : ISignalBounceQueries<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSignalBounceQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }
        

        //Insert
        public virtual async Task Insert(List<SignalBounce<ObjectId>> messages)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _collectionFactory
                .GetCollection<SignalBounce<ObjectId>>()
                .InsertManyAsync(messages, options)
                .ConfigureAwait(false);
        }


        //Select
        /// <summary>
        /// Select SignalBounce items page
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="receiverSubscriberIds"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<SignalBounce<ObjectId>>>> SelectPage(int pageIndex, int pageSize, List<ObjectId> receiverSubscriberIds = null)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);

            var filter = Builders<SignalBounce<ObjectId>>.Filter.Where(p => true);
            if(receiverSubscriberIds != null )
            {
                filter = Builders<SignalBounce<ObjectId>>.Filter.And(filter,
                    Builders<SignalBounce<ObjectId>>.Filter.Where(p => p.ReceiverSubscriberId != null
                    && receiverSubscriberIds.Contains(p.ReceiverSubscriberId.Value)));
            }
            IMongoCollection<SignalBounce<ObjectId>> signalBounces = _collectionFactory
                .GetCollection<SignalBounce<ObjectId>>();

            Task<List<SignalBounce<ObjectId>>> listTask = signalBounces
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> countTask = signalBounces.CountDocumentsAsync(filter);

            long count = await countTask.ConfigureAwait(false);
            List<SignalBounce<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<SignalBounce<ObjectId>>>(list, count);
        }


        //Delete
        public virtual async Task Delete(List<ObjectId> receiverSubscriberIds)
        {
            var filter = Builders<SignalBounce<ObjectId>>.Filter.Where(
                p => p.ReceiverSubscriberId != null 
                && receiverSubscriberIds.Contains(p.ReceiverSubscriberId.Value));

            DeleteResult response = await _collectionFactory
                .GetCollection<SignalBounce<ObjectId>>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(List<SignalBounce<ObjectId>> items)
        {
            List<ObjectId> ids = items
                .Select(p => p.SignalBounceId).ToList();

            var filter = Builders<SignalBounce<ObjectId>>.Filter.Where(
                p => ids.Contains(p.SignalBounceId));

            DeleteResult response = await _collectionFactory
                .GetCollection<SignalBounce<ObjectId>>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
