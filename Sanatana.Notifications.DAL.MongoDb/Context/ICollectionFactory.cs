using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.Context
{
    public interface ICollectionFactory
    {
        IMongoCollection<TEntity> GetCollection<TEntity>();
        IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName);
    }
}
