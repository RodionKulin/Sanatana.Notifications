using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.MongoDb.Repository;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.MongoDb.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbConsolidationLockQueries : MongoDbRepository<ConsolidationLock<ObjectId>>, IConsolidationLockQueries<ObjectId>
    {

        //ctor
        public MongoDbConsolidationLockQueries(ICollectionFactory collectionFactory)
        {
            _collection = collectionFactory.GetCollection<ConsolidationLock<ObjectId>>();
        }


        //find methods
        public virtual Task<ConsolidationLock<ObjectId>> FindExistingMatch(ConsolidationLock<ObjectId> consolidationLock, CancellationToken token = default)
        {
            Expression<Func<ConsolidationLock<ObjectId>, bool>> filter = 
                x => x.ReceiverSubscriberId == consolidationLock.ReceiverSubscriberId
                && x.CategoryId == consolidationLock.CategoryId
                && x.DeliveryType == consolidationLock.DeliveryType;
            return FindOne(filter, token);
        }


        //update methods
        public virtual async Task<bool> ExtendLockTime(ConsolidationLock<ObjectId> lockToExtend, CancellationToken token = default)
        {
            Expression<Func<ConsolidationLock<ObjectId>, bool>> filter =
                x => x.ReceiverSubscriberId == lockToExtend.ReceiverSubscriberId
                && x.CategoryId == lockToExtend.CategoryId
                && x.DeliveryType == lockToExtend.DeliveryType
                && x.LockedSinceUtc == lockToExtend.LockedSinceUtc; //make sure it was not updated by some other Sender instance already

            var updates = Updates<ConsolidationLock<ObjectId>>.Empty()
                .Set(x => x.LockedSinceUtc, DateTime.UtcNow)
                .Set(x => x.LockedBy, lockToExtend.LockedBy);

            long docsUpdates = await UpdateOne(filter, updates, token).ConfigureAwait(false);
            bool lockExtended = docsUpdates > 0;
            return lockExtended;
        }


        //delete methods
        public virtual async Task Delete(ConsolidationLock<ObjectId>[] locksToRemove, CancellationToken token = default)
        {
            var filters = locksToRemove.Select(
                cacheLock => Builders<ConsolidationLock<ObjectId>>.Filter.Where(
                    db => db.ReceiverSubscriberId == cacheLock.ReceiverSubscriberId
                    && db.CategoryId == cacheLock.CategoryId
                    && db.DeliveryType == cacheLock.DeliveryType
                    && db.LockedBy == cacheLock.LockedBy)
                );

            var filter = Builders<ConsolidationLock<ObjectId>>.Filter.Or(filters);
            //string json = FilterDefinitionExtensions.ToJson(filter);

            DeleteResult result = await _collection.DeleteManyAsync(filter, cancellationToken: token)
                .ConfigureAwait(false);
        }
    }
}
