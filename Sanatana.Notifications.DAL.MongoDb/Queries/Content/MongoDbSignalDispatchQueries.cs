using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSignalDispatchQueries : ISignalDispatchQueries<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSignalDispatchQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }



        //Insert
        public virtual async Task Insert(List<SignalDispatch<ObjectId>> items)
        {
            foreach (SignalDispatch<ObjectId> item in items)
            {
                item.SignalDispatchId = ObjectId.GenerateNewId();
            }

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .InsertManyAsync(items, options)
                .ConfigureAwait(false);
        }


        //Select
        public virtual Task<List<SignalDispatch<ObjectId>>> Select(
            int count, List<int> deliveryTypes, int maxFailedAttempts)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                    p => p.SendDateUtc <= DateTime.UtcNow
                    && deliveryTypes.Contains(p.DeliveryType)
                    && p.FailedAttempts < maxFailedAttempts);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter, options)
                .SortBy(p => p.SendDateUtc)
                .Limit(count)
                .ToListAsync();
        }

        public virtual Task<List<SignalDispatch<ObjectId>>> SelectConsolidated(
            int pageSize, List<ObjectId> subscriberIds, List<(int deliveryType, int category)> categories,
            DateTime createdBefore, DateTime? createdAfter = null)
        {
            if (categories.Count == 0)
            {
                return Task.FromResult(new List<SignalDispatch<ObjectId>>());
            }

            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.ReceiverSubscriberId != null
                && subscriberIds.Contains(p.ReceiverSubscriberId.Value)
                && p.CreateDateUtc <= createdBefore);

            if(createdAfter != null)
            {
                filter &= Builders<SignalDispatch<ObjectId>>.Filter
                    .Where(x => createdAfter.Value < x.CreateDateUtc);
            }

            var categoryFilter = Builders<SignalDispatch<ObjectId>>.Filter.Where(p => false);
            foreach ((int deliveryType, int category) pair in categories)
            {
                categoryFilter = Builders<SignalDispatch<ObjectId>>.Filter.Or(categoryFilter, Builders<SignalDispatch<ObjectId>>.Filter.Where(
                    p => p.DeliveryType == pair.deliveryType
                    && p.CategoryId == pair.category));
            }
            filter &= categoryFilter;

            var projection = Builders<SignalDispatch<ObjectId>>.Projection
                .Include(x => x.SignalDispatchId)
                .Include(x => x.CreateDateUtc)
                .Include(x => x.TemplateData);

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter)
                .SortBy(x => x.CreateDateUtc)
                .Limit(pageSize)
                .Project(projection)
                .As<SignalDispatch<ObjectId>>()
                .ToListAsync();
        }


        //Update
        public virtual async Task UpdateSendResults(List<SignalDispatch<ObjectId>> items)
        {
            var operations = new List<WriteModel<SignalDispatch<ObjectId>>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                    p => p.SignalDispatchId == item.SignalDispatchId);

                var update = Builders<SignalDispatch<ObjectId>>.Update
                    .Set(p => p.SendDateUtc, item.SendDateUtc)
                    .Set(p => p.FailedAttempts, item.FailedAttempts)
                    .Set(p => p.IsScheduled, item.IsScheduled);

                operations.Add(new UpdateOneModel<SignalDispatch<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .BulkWriteAsync(operations, options)
                .ConfigureAwait(false);
        }



        //Delete
        public virtual async Task Delete(List<SignalDispatch<ObjectId>> items)
        {
            List<ObjectId> ids = items.Select(p => p.SignalDispatchId).ToList();

            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => ids.Contains(p.SignalDispatchId));

            DeleteResult response = await _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteConsolidated(List<SignalDispatch<ObjectId>> items)
        {
            if(items.Count == 0)
            {
                return;
            }

            var operations = new List<WriteModel<SignalDispatch<ObjectId>>>();
            foreach (SignalDispatch<ObjectId> item in items)
            {
                if(item.ReceiverSubscriberId == null)
                {
                    continue;
                }

                var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                    p => p.ReceiverSubscriberId == item.ReceiverSubscriberId
                    && p.CategoryId == item.CategoryId
                    && p.DeliveryType == item.DeliveryType
                    && p.CreateDateUtc <= item.SendDateUtc
                    && p.SignalDispatchId != item.SignalDispatchId);
                operations.Add(new DeleteManyModel<SignalDispatch<ObjectId>>(filter));
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .BulkWriteAsync(operations, options)
                .ConfigureAwait(false);
        }


        //IDisposable
        public virtual void Dispose()
        {
        }

    }
}
