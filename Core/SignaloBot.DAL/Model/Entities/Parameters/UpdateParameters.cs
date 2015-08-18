using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Parameters
{
    public class UpdateParameters
    {
        //свойства
        public bool UpdateDeliveryTypeLastSendDateUtc { get; set; }
        public bool UpdateCategoryLastSendDateUtc { get; set; }
        public bool UpdateTopicLastSendDateUtc { get; set; }
        
        public bool UpdateDeliveryTypeSendCount { get; set; }
        public bool UpdateCategorySendCount { get; set; }
        public bool UpdateTopicSendCount { get; set; }

        public bool CreateCategoryIfNotExist { get; set; }
        public bool CreateTopicIfNotExist { get; set; }

        //зависимые свойства
        internal bool UpdateDeliveryType
        {
            get
            {
                return UpdateDeliveryTypeLastSendDateUtc || UpdateDeliveryTypeSendCount;
            }
        }
        internal bool UpdateCategory
        {
            get
            {
                return UpdateCategoryLastSendDateUtc || UpdateCategorySendCount;
            }
        }
        internal bool UpdateTopic
        {
            get
            {
                return UpdateTopicLastSendDateUtc || UpdateTopicSendCount;
            }
        }
        internal bool UpdateAnything
        {
            get
            {
                return UpdateDeliveryType || UpdateCategory || UpdateTopic;
            }
        }
    }
}
