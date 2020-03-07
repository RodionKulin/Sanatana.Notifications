using MongoDB.Driver;
using Sanatana.Notifications.DAL.MongoDb;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using StructureMap.AutoMocking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Behaviors
{
    public class DatabasePurger : Behavior<INeedDbContext>
    {
        //fields
        private static bool _isInitialized;


        //methods
        public override void SpecInit(INeedDbContext instance)
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;

            SenderMongoDbContext dbContext = instance.Mocker.GetServiceInstance<SenderMongoDbContext>();
            IMongoDatabase db = dbContext.SubscriberDeliveryTypeSettings.Database;
            db.DropCollection(dbContext.SubscriberDeliveryTypeSettings.CollectionNamespace.CollectionName);
            db.DropCollection(dbContext.SubscriberCategorySettings.CollectionNamespace.CollectionName);
            db.DropCollection(dbContext.SubscriberTopicSettings.CollectionNamespace.CollectionName);
        }
    }
}
