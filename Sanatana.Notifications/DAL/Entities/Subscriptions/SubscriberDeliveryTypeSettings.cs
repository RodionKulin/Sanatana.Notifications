using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class SubscriberDeliveryTypeSettings<TKey>
        where TKey : struct
    {
        //properties
        public TKey SubscriberDeliveryTypeSettingsId { get; set; }
        public TKey SubscriberId { get; set; }
        public TKey? GroupId { get; set; }
        public int DeliveryType { get; set; }
        public string Address { get; set; }
        public string Language { get; set; }
        public string TimeZoneId { get; set; }
        public DateTime? LastVisitUtc { get; set; }
        public DateTime? LastSendDateUtc { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsEnabledOnNewTopic { get; set; }
        public int SendCount { get; set; }
        public int NDRCount { get; set; }
        public bool IsNDRBlocked { get; set; }
        public DateTime? NDRBlockResetCodeSendDateUtc { get; set; }
        public string NDRBlockResetCode { get; set; }

        public List<SubscriberCategorySettings<TKey>> SubscriberCategorySettings { get; set; }


        //init
        public static SubscriberDeliveryTypeSettings<TKey> Create(TKey subscriberId, int deliveryType, string address
            , string language = null, string timeZoneId = null, TKey? groupId = null)
        {
            return new SubscriberDeliveryTypeSettings<TKey>()
            {
                SubscriberId = subscriberId,
                GroupId = groupId,
                DeliveryType = deliveryType,
                Address = address,
                Language = language,
                TimeZoneId = timeZoneId,

                LastVisitUtc = null,
                LastSendDateUtc = null,
                IsEnabled = true,
                IsEnabledOnNewTopic = true,
                SendCount = 0,

                NDRCount = 0,
                IsNDRBlocked = false,
                NDRBlockResetCode = null,
                NDRBlockResetCodeSendDateUtc = null
            };
        }
    }
}
