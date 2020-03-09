using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
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
        SpecsDbContext DbContext { get; set; }
    }
}
