using NUnit.Framework;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Providers;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class EfTestConfig : SpecsForConfiguration
{
    public EfTestConfig()
    {
        WhenTesting<INeedDbContext>().EnrichWith<DependenciesProvider>();
        WhenTesting<INeedDbContext>().EnrichWith<DatabaseCreator>();
        WhenTesting<INeedDbContext>().EnrichWith<DataPurger>();
    }
}
