using MongoDB.Bson;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.MongoDb.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects
{
    public class SpecsDbInitializer : SenderMongoDbInitializer<
        MongoDbSubscriberDeliveryTypeSettings<SubscriberCategorySettings<ObjectId>>,
        SubscriberCategorySettings<ObjectId>,
        SubscriberTopicSettings<ObjectId>>
    {

        public SpecsDbInitializer(SpecsDbContext context)
            :base (context)
        { 
        }
    }

}
