using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using StructureMap.AutoMocking;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDb.Context;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Behaviors
{
    public class DependenciesProvider : Behavior<INeedDbContext>
    {
        private bool _serializerConfigured;

        //methods
        public override void SpecInit(INeedDbContext instance)
        {
            if (!_serializerConfigured)
            {
                SpecsDbContext.ApplyGlobalSerializationSettings();
                _serializerConfigured = true;
            }

            var connection = new MongoDbConnectionSettings
            {
                DatabaseName = "NotificationsSpecs",
                Host = "localhost",
                Port = 27017
            };

            AutoMockedContainer autoMockContainer = instance.Mocker.GetContainer();
            autoMockContainer.Configure(cfg =>
            {
                cfg.For<MongoDbConnectionSettings>().Use(connection);
                cfg.For<SpecsDbContext>().Use<SpecsDbContext>();
                cfg.For<ICollectionFactory>().Use<SpecsDbContext>();
            });

            instance.DbContext = instance.Mocker.GetServiceInstance<SpecsDbContext>();
        }
    }
}
