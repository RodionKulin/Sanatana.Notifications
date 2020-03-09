using MongoDB.Bson;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.MongoDb.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects
{
    public class SpecsDeliveryTypeSettings : MongoDbSubscriberDeliveryTypeSettings<SubscriberCategorySettings<ObjectId>>
    {
    }
}
