using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces
{
    public interface INeedDbContext : ISpecs
    {
        SenderDbContext DbContext { get; set; }
    }
}
