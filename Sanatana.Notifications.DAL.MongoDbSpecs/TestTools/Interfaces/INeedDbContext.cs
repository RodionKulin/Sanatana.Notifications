using Sanatana.Notifications.DAL.MongoDb;
using SpecsFor;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces
{
    public interface INeedDbContext : ISpecs
    {
        SenderMongoDbContext DbContext { get; set; }
    }
}
