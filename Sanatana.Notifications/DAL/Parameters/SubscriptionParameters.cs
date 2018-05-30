using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Parameters
{
    public class SubscriptionParameters
    {
        //match
        public int? DeliveryType { get; set; }
        public int? CategoryId { get; set; }
        public string TopicId { get; set; }
        
        //latest delivery is lower than visit time
        public bool CheckDeliveryTypeLastSendDate { get; set; }
        public bool CheckCategoryLastSendDate { get; set; }
        public bool CheckTopicLastSendDate { get; set; }
        
        //enabled
        public bool CheckDeliveryTypeEnabled { get; set; }
        public bool CheckCategoryEnabled { get; set; }
        public bool CheckTopicEnabled { get; set; }

        //limits on number of signals
        public int? CheckDeliveryTypeSendCountNotGreater { get; set; }
        public int? CheckCategorySendCountNotGreater { get; set; }
        public int? CheckTopicSendCountNotGreater { get; set; }

        //is blocked
        public bool CheckIsNDRBlocked { get; set; }
    }
}
