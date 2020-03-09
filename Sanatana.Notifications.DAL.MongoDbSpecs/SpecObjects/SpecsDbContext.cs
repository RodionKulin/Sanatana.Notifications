using MongoDB.Bson;
using Sanatana.MongoDb;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.MongoDb.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects
{
    public class SpecsDbContext : SenderMongoDbContext<
        MongoDbSubscriberDeliveryTypeSettings<SubscriberCategorySettings<ObjectId>>,
        SubscriberCategorySettings<ObjectId>,
        SubscriberTopicSettings<ObjectId>>
    {

        public SpecsDbContext(MongoDbConnectionSettings settings)
            : base(settings)
        {

        }
    }
}
