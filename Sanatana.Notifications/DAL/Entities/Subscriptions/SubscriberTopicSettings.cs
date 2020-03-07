using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sanatana.Notifications.DAL.Entities
{
    public class SubscriberTopicSettings<TKey>
        where TKey : struct
    {
        //properties
        public TKey SubscriberTopicSettingsId { get; set; }
        public TKey SubscriberId { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryId { get; set; }
        public string TopicId { get; set; }

        public DateTime AddDateUtc { get; set; }

        public DateTime? LastSendDateUtc { get; set; }
        public int SendCount { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }



        //init
        public static SubscriberTopicSettings<TKey> Create(
            TKey subscriberId, int deliveryType, int categoryId, string topicId)
        {
            return new SubscriberTopicSettings<TKey>()
            {
                SubscriberId = subscriberId,
                DeliveryType = deliveryType,
                CategoryId = categoryId,
                TopicId = topicId,

                LastSendDateUtc = null,
                SendCount = 0,
                IsEnabled = true,
                IsDeleted = false
            };
        }
    }
}