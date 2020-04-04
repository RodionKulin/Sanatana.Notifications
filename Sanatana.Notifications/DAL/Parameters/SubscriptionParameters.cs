using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Parameters
{
    public class SubscriptionParameters
    {
        //Match id
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

        //Latest delivery is lower than visit time
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
        
        //Is enabled
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

        //Limits on number of signals
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

        //Is blocked
        /// <summary>
        /// Match subscribers that have delivery type not blocked because of NDR incoming.
        /// </summary>
        public bool CheckIsNDRBlocked { get; set; }

        //Custom filter values
        /// <summary>
        /// Filters data that can be used in custom subscriber filtering code. Override ISubscriberQueries methods to use.
        /// </summary>
        public Dictionary<string, string> SubscriberFiltersData { get; set; }
    }
}
