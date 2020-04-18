using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class SubscriberCategorySettings<TKey>
        where TKey : struct
    {
        //properties
        public TKey SubscriberCategorySettingsId { get; set; }
        public TKey SubscriberId { get; set; }
        public int DeliveryType { get; set; }
        public int CategoryId { get; set; }

        public DateTime? LastSendDateUtc { get; set; }
        public int SendCount { get; set; }
        public bool IsEnabled { get; set; }
        


        //init
        public static SubscriberCategorySettings<TKey> Create(
            TKey subscriberId, int deliveryType, int categoryId)
        {
            return new SubscriberCategorySettings<TKey>()
            {
                SubscriberId = subscriberId,
                DeliveryType = deliveryType,
                CategoryId = categoryId,

                LastSendDateUtc = null,
                SendCount = 0,
                IsEnabled = true,
            };
        }

    }
}
