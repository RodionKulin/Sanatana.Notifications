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

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSignalBounceQueries : ISignalBounceQueries<ObjectId>
    {  
        //fields
        protected MongoDbConnectionSettings _settings;        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSignalBounceQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }
        

        //Insert
        public virtual async Task Insert(List<SignalBounce<ObjectId>> messages)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.SignalBounces.InsertManyAsync(messages, options).ConfigureAwait(false);
        }


        //Select
        /// <summary>
        /// Select SignalBounce items page
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="receiverSubscriberIds"></param>
        /// <returns></returns>
        public async Task<TotalResult<List<SignalBounce<ObjectId>>>> SelectPage(int pageIndex, int pageSize, List<ObjectId> receiverSubscriberIds = null)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);

            var filter = Builders<SignalBounce<ObjectId>>.Filter.Where(p => true);
            if(receiverSubscriberIds != null )
            {
                filter = Builders<SignalBounce<ObjectId>>.Filter.And(filter,
                    Builders<SignalBounce<ObjectId>>.Filter.Where(p => p.ReceiverSubscriberId != null
                    && receiverSubscriberIds.Contains(p.ReceiverSubscriberId.Value)));
            }

            Task<List<SignalBounce<ObjectId>>> listTask = _context.SignalBounces
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            Task<long> countTask = _context.SignalBounces.CountDocumentsAsync(filter);

            long count = await countTask.ConfigureAwait(false);
            List<SignalBounce<ObjectId>> list = await listTask.ConfigureAwait(false);

            return new TotalResult<List<SignalBounce<ObjectId>>>(list, count);
        }


        //Delete
        public async Task Delete(List<ObjectId> receiverSubscriberIds)
        {
            var filter = Builders<SignalBounce<ObjectId>>.Filter.Where(
                p => p.ReceiverSubscriberId != null 
                && receiverSubscriberIds.Contains(p.ReceiverSubscriberId.Value));

            DeleteResult response = await _context.SignalBounces.DeleteManyAsync(filter).ConfigureAwait(false);
        }

        public async Task Delete(List<SignalBounce<ObjectId>> items)
        {
            List<ObjectId> ids = items
                .Select(p => p.SignalBounceId).ToList();

            var filter = Builders<SignalBounce<ObjectId>>.Filter.Where(
                p => ids.Contains(p.SignalBounceId));

            DeleteResult response = await _context.SignalBounces.DeleteManyAsync(filter).ConfigureAwait(false);
        }

    }
}
