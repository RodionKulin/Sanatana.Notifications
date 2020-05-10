using MongoDB.Driver;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Strategies;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration
{
    public class GeneratorRunner
    {
        //fields
        protected long _flushAfterNumberOfEntities = 50000;


        //generate entry point
        public InMemoryStorage Generate(SpecsDbContext dbContext, IGeneratorData generatorData, 
            bool useMemoryStorage, Dictionary<Type, long> ammounts)
        {
            var setup = new GeneratorSetup();
            generatorData.RegisterEntities(setup, dbContext);
            SetDataAmounts(setup, ammounts);
            InMemoryStorage storage = useMemoryStorage
                ? SetMemoryStorage(setup, ammounts)
                : null;

            setup.ProgressChanged += PrintProgress;
            setup.Generate();

            return storage;
        }


        //prepare settings
        protected virtual void SetDataAmounts(GeneratorSetup setup, Dictionary<Type, long> dataAmmounts)
        {
            foreach (Type type in dataAmmounts.Keys)
            {
                IEntityDescription entityDescription = GetEntityDescription(setup, type);
                long ammount = dataAmmounts[type];

                PropertyInfo quantityPropInfo = entityDescription.GetType()
                    .GetProperty(nameof(entityDescription.QuantityProvider));
                quantityPropInfo.SetValue(entityDescription, new StrictQuantityProvider(ammount));

                PropertyInfo flushTriggerPropInfo = entityDescription.GetType()
                    .GetProperty(nameof(entityDescription.FlushTrigger));
                flushTriggerPropInfo.SetValue(entityDescription, new LimitedCapacityFlushStrategy(ammount));
            }
        }

        protected virtual IEntityDescription GetEntityDescription(GeneratorSetup setup, Type entityType)
        {
            bool isEntityRegistered = setup.EntityDescriptions.ContainsKey(entityType);
            if (!isEntityRegistered || setup.EntityDescriptions[entityType] == null)
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered.");
            }
            return setup.EntityDescriptions[entityType];
        }
        
        protected virtual InMemoryStorage SetMemoryStorage(GeneratorSetup setup, Dictionary<Type, long> dataAmmounts)
        {
            var storage = new InMemoryStorage();
            foreach (Type type in dataAmmounts.Keys)
            {
                IEntityDescription entityDescription = GetEntityDescription(setup, type);
                entityDescription.PersistentStorages = entityDescription.PersistentStorages ?? new List<IPersistentStorage>();
                entityDescription.PersistentStorages.Add(storage);
            }
            return storage;
        }


        //progress
        private void PrintProgress(GeneratorSetup setup, decimal percent)
        {
            Debug.WriteLine($"Data generation progress {percent.ToString("F")}%");
        }
    }
}
