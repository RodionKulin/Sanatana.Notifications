using NUnit.Framework;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Behaviors;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.DataGenerationBehaviors;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[SetUpFixture]
public class MongoDbTestConfig : SpecsForConfiguration
{
    public MongoDbTestConfig()
    {
        WhenTestingAnything().EnrichWith<DependenciesProvider>();
        WhenTestingAnything().EnrichWith<DatabasePurger>();
        WhenTesting<INeedDbContext>().EnrichWith<DbContextProvider>();
        WhenTesting<INeedDispatchesData>().EnrichWith<DispatchesCollectionGenerator>();
        WhenTesting<INeedSubscriptionsData>().EnrichWith<CatCollectionGenerator>();
        //WhenTestingAnything().EnrichWith<IndexesCreator>();
        WhenTestingAnything().EnrichWith<LogExecutionTimeBehavior>();
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        LogExecutionTimeBehavior.PrintAll();
    }
}
