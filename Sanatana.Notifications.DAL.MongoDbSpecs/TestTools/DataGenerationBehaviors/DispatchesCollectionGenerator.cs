using MongoDB.Bson;
using Sanatana.DataGenerator.Storages;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGenerationBehaviors
{
    public class DispatchesCollectionGenerator : Behavior<INeedDispatchesData>
    {
        //fields
        protected bool _isInitialized;
        protected InMemoryStorage _storage;


        //methods
        public override void SpecInit(INeedDispatchesData instance)
        {
            if (!_isInitialized)
            {
                _storage = SetupGenerator(instance.Mocker.GetServiceInstance<SpecsDbContext>());
                _isInitialized = true;
            }

            instance.DispatchesGenerated = _storage;
        }

        private InMemoryStorage SetupGenerator(SpecsDbContext dbContext)
        {
            var ammounts = new Dictionary<Type, long>
            {
                [typeof(SignalDispatch<ObjectId>)] = 1000,
            };
            return new GeneratorRunner().Generate(
                dbContext: dbContext, 
                generatorData: new DispatchesData(), 
                useMemoryStorage: true, 
                ammounts: ammounts);
        }

    }
}
