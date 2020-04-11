using MongoDB.Bson;
using Sanatana.Notifications.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Sanatana.MongoDb.Repository;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDb.Queries.Content
{
    public class MongoDbSignalDispatchHistoryQueries : MongoDbRepository<SignalDispatch<ObjectId>>, ISignalDispatchHistoryQueries<ObjectId>
    {

        //ctor
        public MongoDbSignalDispatchHistoryQueries(ICollectionFactory collectionFactory)
        {
            _collection = collectionFactory.GetCollection<SignalDispatch<ObjectId>>(CollectionNames.DISPATCHES_HISTORY);
        }
    }
}
