using Sanatana.DataGenerator.Storages;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces
{
    public interface INeedSubscriptionsData : ISpecs
    {
        public InMemoryStorage SubscribersGenerated { get; set; }
    }
}
