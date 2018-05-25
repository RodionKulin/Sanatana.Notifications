using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public class MongoDbSignalEventQueries : ISignalEventQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSignalEventQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<SignalEvent<ObjectId>> items)
        {
            foreach (SignalEvent<ObjectId> item in items)
            {
                item.SignalEventId = ObjectId.GenerateNewId();
            }

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.SignalEvents.InsertManyAsync(items, options);
        }

        public virtual async Task<List<SignalEvent<ObjectId>>> Select(
            int count, int maxFailedAttempts)
        {
            var filter = Builders<SignalEvent<ObjectId>>.Filter.Where(
                p => p.FailedAttempts < maxFailedAttempts);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            List<SignalEvent<ObjectId>> list = await _context.SignalEvents.Find(filter, options)
                .SortBy(p => p.CreateDateUtc)
                .Limit(count)
                .ToListAsync();

            return list;
        }

        public virtual async Task UpdateSendResults(List<SignalEvent<ObjectId>> items)
        {
            var requests = new List<WriteModel<SignalEvent<ObjectId>>>();

            foreach (SignalEvent<ObjectId> item in items)
            {
                var filter = Builders<SignalEvent<ObjectId>>.Filter.Where(
                    p => p.SignalEventId == item.SignalEventId);

                var update = Builders<SignalEvent<ObjectId>>.Update
                    .Set(p => p.FailedAttempts, item.FailedAttempts)
                    .Set(p => p.ComposerSettingsId, item.ComposerSettingsId)
                    .Set(p => p.SubscriberIdRangeFrom, item.SubscriberIdRangeFrom)
                    .Set(p => p.SubscriberIdRangeTo, item.SubscriberIdRangeTo)
                    .Set(p => p.SubscriberIdFromDeliveryTypesHandled, item.SubscriberIdFromDeliveryTypesHandled);

                requests.Add(new UpdateOneModel<SignalEvent<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.SignalEvents
                .BulkWriteAsync(requests, options);
        }

        public virtual async Task Delete(List<SignalEvent<ObjectId>> items)
        {
            List<ObjectId> Ids = items.Select(p => p.SignalEventId).ToList();

            var filter = Builders<SignalEvent<ObjectId>>.Filter.Where(
                p => Ids.Contains(p.SignalEventId));

            DeleteResult response = await _context.SignalEvents.DeleteManyAsync(filter);
        }



        public virtual void Dispose()
        {
        }
    }
}
