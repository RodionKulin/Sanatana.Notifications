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
        /// <summary>
        /// Find subscriber settings with matching DeliveryType
        /// </summary>
        public int? DeliveryType { get; set; }
        /// <summary>
        /// Check Subscribers category settings with matching CategoryId
        /// </summary>
        public int? CategoryId { get; set; }
        /// <summary>
        /// Check Subscribers topic settings with matching TopicId. Can be overriden from incoming SignalEvent.
        /// </summary>
        public string TopicId { get; set; }

        //latest delivery is lower than visit time
        /// <summary>
        /// Match subscribers that have delivery last send date lower than latest visit time or delivery last send date is empty.
        /// </summary>
        public bool CheckDeliveryTypeLastSendDate { get; set; }
        /// <summary>
        /// Match subscribers that have category last send date lower than latest visit time or category last send date is empty.
        /// </summary>
        public bool CheckCategoryLastSendDate { get; set; }
        /// <summary>
        /// Match subscribers that have topic last send date lower than latest visit time or topic last send date is empty.
        /// </summary>
        public bool CheckTopicLastSendDate { get; set; }
        
        //enabled
        /// <summary>
        /// Match subscribers that have delivery type enabled.
        /// </summary>
        public bool CheckDeliveryTypeEnabled { get; set; }
        /// <summary>
        /// Match subscribers that have category enabled.
        /// </summary>
        public bool CheckCategoryEnabled { get; set; }
        /// <summary>
        /// Match subscribers that have topic enabled.
        /// </summary>
        public bool CheckTopicEnabled { get; set; }

        //limits on number of signals
        /// <summary>
        /// Match subscribers that have delivery type dispatches sent lower or equal to parameter.
        /// </summary>
        public int? CheckDeliveryTypeSendCountNotGreater { get; set; }
        /// <summary>
        /// Match subscribers that have category dispatches sent lower or equal to parameter.
        /// </summary>
        public int? CheckCategorySendCountNotGreater { get; set; }
        /// <summary>
        /// Match subscribers that have topic dispatches sent lower or equal to parameter.
        /// </summary>
        public int? CheckTopicSendCountNotGreater { get; set; }

        //is blocked
        /// <summary>
        /// Match subscribers that have delivery type not blocked because of NDR incoming.
        /// </summary>
        public bool CheckIsNDRBlocked { get; set; }
    }
}
