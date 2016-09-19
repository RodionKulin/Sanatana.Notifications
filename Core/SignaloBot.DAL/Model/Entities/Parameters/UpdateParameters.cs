using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class UpdateParameters
    {
        //свойства
        public virtual bool UpdateDeliveryTypeLastSendDateUtc { get; set; }
        public virtual bool UpdateCategoryLastSendDateUtc { get; set; }
        public virtual bool UpdateTopicLastSendDateUtc { get; set; }
        
        public virtual bool UpdateDeliveryTypeSendCount { get; set; }
        public virtual bool UpdateCategorySendCount { get; set; }
        public virtual bool UpdateTopicSendCount { get; set; }

        public virtual bool CreateCategoryIfNotExist { get; set; }
        public virtual bool CreateTopicIfNotExist { get; set; }


        //зависимые свойства
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
