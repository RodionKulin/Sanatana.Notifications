using MongoDB.Bson;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.Entities
{
    public class MongoDbSubscriberDeliveryTypeSettings<TCategory> : SubscriberDeliveryTypeSettings<ObjectId>
        where TCategory : SubscriberCategorySettings<ObjectId>
    {
        public List<TCategory> SubscriberCategorySettings { get; set; }
    }
}
