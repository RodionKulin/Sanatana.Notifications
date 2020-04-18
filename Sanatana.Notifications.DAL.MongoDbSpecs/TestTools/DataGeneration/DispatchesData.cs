using MongoDB.Driver;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.MongoDb;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.Notifications.DAL.Entities;
using MongoDB.Bson;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration
{
    public class DispatchesData : IGeneratorData
    {
        //register generators
        public virtual void RegisterEntities(GeneratorSetup setup, SpecsDbContext dbContext)
        {
            IMongoDatabase _database = dbContext.SignalBounces.Database;

            setup.RegisterEntity<SignalDispatch<ObjectId>>()
                .SetGenerator(GenerateDispatches)
                .SetPersistentStorage(new MongoDbPersistentStorage(_database, dbContext.SignalDispatches.CollectionNamespace.CollectionName));
        }


        //generators
        private SignalDispatch<ObjectId> GenerateDispatches(GeneratorContext context)
        {
            return new SignalDispatch<ObjectId>()
            {
                SignalDispatchId = ObjectId.GenerateNewId(),
                ReceiverSubscriberId = ObjectId.GenerateNewId(),
                CategoryId = 1,
                DeliveryType = 1,
                CreateDateUtc = DateTime.UtcNow.AddDays(-1)
            };
        }
    }
}
