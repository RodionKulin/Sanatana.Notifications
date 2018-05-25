using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Parameters
{
    public class UpdateParameters
    {
        //properties
        public bool UpdateDeliveryTypeLastSendDateUtc { get; set; }
        public bool UpdateCategoryLastSendDateUtc { get; set; }
        public bool UpdateTopicLastSendDateUtc { get; set; }
        
        public bool UpdateDeliveryTypeSendCount { get; set; }
        public bool UpdateCategorySendCount { get; set; }
        public bool UpdateTopicSendCount { get; set; }

        public bool CreateCategoryIfNotExist { get; set; }
        public bool CreateTopicIfNotExist { get; set; }


        //dependent properties
        public virtual bool UpdateDeliveryType
        {
            get
            {
                return UpdateDeliveryTypeLastSendDateUtc || UpdateDeliveryTypeSendCount;
            }
        }
        public virtual bool UpdateCategory
        {
            get
            {
                return UpdateCategoryLastSendDateUtc || UpdateCategorySendCount;
            }
        }
        public virtual bool UpdateTopic
        {
            get
            {
                return UpdateTopicLastSendDateUtc || UpdateTopicSendCount;
            }
        }
        public virtual bool UpdateAnything
        {
            get
            {
                return UpdateDeliveryType || UpdateCategory || UpdateTopic;
            }
        }
    }
}
