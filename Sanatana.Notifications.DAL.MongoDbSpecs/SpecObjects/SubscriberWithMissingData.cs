using MongoDB.Bson;
using Newtonsoft.Json;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects
{
    public class SubscriberWithMissingData
    {
        public ObjectId SubscriberId { get; set; }
        public bool HasAddress { get; set; }
        public bool HasDeliveryTypeSettings { get; set; }
        public bool HasCategorySettingsEnabled { get; set; }
        public bool HasTopicsSettingsEnabled { get; set; }
        public bool HasGroupId { get; set; }
        public List<SubscriberDeliveryTypeSettings<ObjectId>> DeliveryTypes { get; set; }
        public List<SubscriberCategorySettings<ObjectId>> Categories { get; set; }
        public List<SubscriberTopicSettings<ObjectId>> Topics { get; set; }
        public bool HasTopicLastSendDate { get; set; }
        public bool HasVisitDateFuture { get; set; }
        public bool HasVisitDatePast { get; internal set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
