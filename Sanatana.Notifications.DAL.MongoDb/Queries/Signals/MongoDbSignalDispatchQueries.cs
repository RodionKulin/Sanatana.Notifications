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
using Sanatana.Notifications.DAL.Parameters;
using System.ComponentModel;

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
        protected virtual FilterDefinition<SignalDispatch<ObjectId>> ToFilter(DispatchQueryParameters<ObjectId> parameters)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
              p => p.SendDateUtc <= DateTime.UtcNow
              && parameters.ActiveDeliveryTypes.Contains(p.DeliveryType)
              && p.FailedAttempts < parameters.MaxFailedAttempts
              && !parameters.ExcludeIds.Contains(p.SignalDispatchId));

            foreach (ConsolidationLock<ObjectId> locked in parameters.ExcludeConsolidated)
            {
                filter &= Builders<SignalDispatch<ObjectId>>.Filter.Where(
                   x => x.ReceiverSubscriberId != locked.ReceiverSubscriberId
                   || x.CategoryId != locked.CategoryId
                   || x.DeliveryType != locked.DeliveryType);
            }

            //string json = FilterDefinitionExtensions.ToJson(filter);

            return filter;
        }

        public virtual Task<List<SignalDispatch<ObjectId>>> SelectNotSetLock(DispatchQueryParameters<ObjectId> parameters)
        {
            FilterDefinition<SignalDispatch<ObjectId>> filter = ToFilter(parameters);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter, options)
                .SortBy(p => p.SendDateUtc)
                .Limit(parameters.Count)
                .ToListAsync();
        }

        public virtual async Task<List<SignalDispatch<ObjectId>>> SelectWithSetLock(DispatchQueryParameters<ObjectId> parameters, Guid lockId, DateTime lockExpirationDate)
        {
            //same date as stored in LockTracker
            DateTime lockStartTimeUtc = DateTime.UtcNow;

            List<SignalDispatch<ObjectId>> selected = await SelectUnlocked(parameters, lockExpirationDate)
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

            return await SelectLocked(parameters, lockId, lockExpirationDate).ConfigureAwait(false);
        }

        public virtual Task<List<SignalDispatch<ObjectId>>> SelectUnlocked(DispatchQueryParameters<ObjectId> parameters, DateTime lockExpirationDate)
        {
            FilterDefinition<SignalDispatch<ObjectId>> filter = ToFilter(parameters);
            filter &= Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.LockedBy == null
                || p.LockedSinceUtc == null
                || p.LockedSinceUtc < lockExpirationDate);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            //string filterJson = FilterDefinitionExtensions.ToJson(filter);

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter, options)
                .SortBy(p => p.SendDateUtc)
                .Limit(parameters.Count)
                .ToListAsync();
        }

        public virtual Task<List<SignalDispatch<ObjectId>>> SelectLocked(DispatchQueryParameters<ObjectId> parameters, Guid lockId, DateTime lockExpirationDate)
        {
            FilterDefinition<SignalDispatch<ObjectId>> filter = ToFilter(parameters);
            filter &= Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => p.LockedBy == lockId
                && p.LockedSinceUtc != null
                && p.LockedSinceUtc >= lockExpirationDate);

            var options = new FindOptions()
            {
                AllowPartialResults = true
            };

            return _collectionFactory
                .GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES)
                .Find(filter, options)
                .SortBy(x => x.SendDateUtc)
                .Limit(parameters.Count)
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
                    .Set(p => p.LockedSinceUtc, item.LockedSinceUtc);

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

        public virtual async Task<bool> SetLock(List<ObjectId> dispatchIds, Guid lockId, DateTime newLockSinceTimeUtc, DateTime existingLockSinceDateUtc)
        {
            var filter = Builders<SignalDispatch<ObjectId>>.Filter.Where(
                p => dispatchIds.Contains(p.SignalDispatchId));

            var lockFilter = Builders<SignalDispatch<ObjectId>>.Filter.Where(p => p.LockedBy == null);
            lockFilter |= Builders<SignalDispatch<ObjectId>>.Filter.Where(p => p.LockedSinceUtc == null);
            lockFilter |= Builders<SignalDispatch<ObjectId>>.Filter.Where(p => p.LockedSinceUtc < existingLockSinceDateUtc);
            filter &= lockFilter;

            var update = Builders<SignalDispatch<ObjectId>>.Update
                .Set(p => p.LockedBy, lockId)
                .Set(p => p.LockedSinceUtc, newLockSinceTimeUtc);

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
