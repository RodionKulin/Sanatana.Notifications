using Sanatana.DataGenerator;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGeneration
{
    public interface IGeneratorData
    {
        void RegisterEntities(GeneratorSetup setup, SpecsDbContext dbContext);
    }
}
