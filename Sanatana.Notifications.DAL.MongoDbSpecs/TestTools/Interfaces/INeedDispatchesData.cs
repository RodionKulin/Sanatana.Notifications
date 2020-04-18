using Sanatana.DataGenerator.Storages;
using SpecsFor.Core;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces
{
    public interface INeedDispatchesData : ISpecs
    {
        public InMemoryStorage DispatchesGenerated { get; set; }
    }
}