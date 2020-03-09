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
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSignalEventQueries : ISignalEventQueries<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSignalEventQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
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

            await _collectionFactory
                .GetCollection<SignalEvent<ObjectId>>()
                .InsertManyAsync(items, options)
                .ConfigureAwait(false);
        }

        public virtual async Task<List<SignalEvent<ObjectId>>> Find(int count, int maxFailedAttempts)
        {
            var filter = Builders<SignalEvent<ObjectId>>.Filter.Where(
                p => p.FailedAttempts < maxFailedAttempts);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            List<SignalEvent<ObjectId>> list = await _collectionFactory
                .GetCollection<SignalEvent<ObjectId>>()
                .Find(filter, options)
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
                    .Set(p => p.EventSettingsId, item.EventSettingsId)
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

            BulkWriteResult response = await _collectionFactory
                .GetCollection<SignalEvent<ObjectId>>()
                .BulkWriteAsync(requests, options);
        }

        public virtual async Task Delete(List<SignalEvent<ObjectId>> items)
        {
            List<ObjectId> Ids = items.Select(p => p.SignalEventId).ToList();

            var filter = Builders<SignalEvent<ObjectId>>.Filter.Where(
                p => Ids.Contains(p.SignalEventId));

            DeleteResult response = await _collectionFactory
                .GetCollection<SignalEvent<ObjectId>>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }



        public virtual void Dispose()
        {
        }
    }
}
