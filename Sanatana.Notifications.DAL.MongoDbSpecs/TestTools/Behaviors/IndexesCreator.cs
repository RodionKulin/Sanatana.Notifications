using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Behaviors
{
    public class IndexesCreator : Behavior<ISpecs>
    {
        //fields
        private bool _isInitialized;


        //methods
        public override void SpecInit(ISpecs instance)
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;

            CreateIndexes(instance);
        }

        protected virtual void CreateIndexes(ISpecs instance)
        {
            var connectionSettings = instance.Mocker.GetServiceInstance<MongoDbConnectionSettings>();
            var context = new SpecsDbContext(connectionSettings);
            var indexCreator = new SpecsDbInitializer(context);
            indexCreator.CreateAllIndexes(useGroupId: false);
        }
    }
}
