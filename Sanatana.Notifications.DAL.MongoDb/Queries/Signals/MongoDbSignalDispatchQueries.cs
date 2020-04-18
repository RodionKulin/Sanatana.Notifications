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
using MongoDB.Driver.Core.Operations;
using System.Threading;
using MongoDB.Driver.Core.Bindings;
using Sanatana.MongoDb.Extensions;
using System.Linq.Expressions;

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
        public virtual Task<List<SignalDispatch<ObjectId>>> Select(int count, List<int> deliveryTypes, 
            int maxFailedAttempts, ObjectId[] excludeIds)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.SendDateUtc <= DateTime.UtcNow
                && deliveryTypes.Contains(p.DeliveryType)
                && p.FailedAttempts < maxFailedAttempts
                && !excludeIds.Contains(p.SignalDispatchId));

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

        public virtual async Task<List<SignalDispatch<ObjectId>>> SelectWithSetLock(int count, List<int> deliveryTypes,
            int maxFailedAttempts, ObjectId[] excludeIds, Guid lockId, DateTime lockExpirationDate)
        {
            //same date as stored in LockTracker
            DateTime lockStartTimeUtc = DateTime.UtcNow;

            List<SignalDispatch<ObjectId>> selected = await SelectUnlocked(count, deliveryTypes, maxFailedAttempts, excludeIds, lockExpirationDate)
                .ConfigureAwait(false);
            if(selected.Count == 0)
            {
                return selected;
            }

            List<ObjectId> dispatchIds = selected.Select(x => x.SignalDispatchId).ToList();
            bool lockSetOnAllEntities = await SetLock(dispatchIds, lockId, lockStartTimeUtc, lockExpirationDate)
                .ConfigureAwait(false);
            if (lockSetOnAllEntities)
            {
                return selected;
            }

            return await SelectLocked(count, deliveryTypes, maxFailedAttempts, excludeIds, lockId, lockExpirationDate)
                .ConfigureAwait(false);
        }

        public virtual Task<List<SignalDispatch<ObjectId>>> SelectUnlocked(int count, List<int> deliveryTypes,
            int maxFailedAttempts, ObjectId[] excludeIds, DateTime lockExpirationDate)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.SendDateUtc <= DateTime.UtcNow
                && deliveryTypes.Contains(p.DeliveryType)
                && p.FailedAttempts < maxFailedAttempts
                && !excludeIds.Contains(p.SignalDispatchId));

            filter &= Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.LockedBy == null
                || p.LockedDateUtc == null
                || p.LockedDateUtc < lockExpirationDate);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            //string filterJson = FilterDefinitionExtensions.ToJson(filter);

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter, options)
                .SortBy(p => p.SendDateUtc)
                .Limit(count)
                .ToListAsync();
        }

        public virtual Task<List<SignalDispatch<ObjectId>>> SelectLocked(int count, List<int> deliveryTypes,
            int maxFailedAttempts, ObjectId[] excludeIds, Guid lockId, DateTime lockExpirationDate)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.SendDateUtc <= DateTime.UtcNow
                && deliveryTypes.Contains(p.DeliveryType)
                && p.FailedAttempts < maxFailedAttempts
                && !excludeIds.Contains(p.SignalDispatchId)
                && p.LockedBy == lockId
                && p.LockedDateUtc != null 
                && p.LockedDateUtc >= lockExpirationDate);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter, options)
                .SortBy(x => x.SendDateUtc)
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
                    .Set(p => p.IsScheduled, item.IsScheduled)
                    .Set(p => p.LockedBy, item.LockedBy)
                    .Set(p => p.LockedDateUtc, item.LockedDateUtc);

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

        public virtual async Task<bool> SetLock(List<ObjectId> dispatchIds, Guid lockId, DateTime lockStartTime, DateTime lockExpirationDate)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => dispatchIds.Contains(p.SignalDispatchId));

            var lockFilter = Builders<SignalDispatch<ObjectId>>.Filter.Where(p => p.LockedBy == null);
            lockFilter |= Builders<SignalDispatch<ObjectId>>.Filter.Where(p => p.LockedDateUtc == null);
            lockFilter |= Builders<SignalDispatch<ObjectId>>.Filter.Where(p => p.LockedDateUtc < lockExpirationDate);
            filter &= lockFilter;

            var update = Builders<SignalDispatch<ObjectId>>.Update
                .Set(p => p.LockedBy, lockId)
                .Set(p => p.LockedDateUtc, lockStartTime);

            var options = new UpdateOptions
            {
                IsUpsert = false
            };

            UpdateResult updateResult = await _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .UpdateManyAsync(filter, update, options)
                .ConfigureAwait(false);

            //string filterJson = FilterDefinitionExtensions.ToJson(filter);

            return updateResult.ModifiedCount == dispatchIds.Count;
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
