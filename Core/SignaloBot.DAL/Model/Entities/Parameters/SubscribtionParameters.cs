using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class SubscribtionParameters
    {
        //тип доставки, категория, тема
        public virtual int? DeliveryType { get; set; }
        public virtual int? CategoryID { get; set; }
        public virtual string TopicID { get; set; }
                
        //проверять время последней отправки
        public virtual bool CheckDeliveryTypeLastSendDate { get; set; }
        public virtual bool CheckCategoryLastSendDate { get; set; }
        public virtual bool CheckTopicLastSendDate { get; set; }
        
        //проверять включения (enabled)
        public virtual bool CheckDeliveryTypeEnabled { get; set; }
        public virtual bool CheckCategoryEnabled { get; set; }
        public virtual bool CheckTopicEnabled { get; set; }

        //проверять лимит на отправку сообщений
        public virtual int? CheckDeliveryTypeSendCountNotGreater { get; set; }
        public virtual int? CheckCategorySendCountNotGreater { get; set; }
        public virtual int? CheckTopicSendCountNotGreater { get; set; }

        //проверять NDR блок
        public virtual bool CheckBlockedOfNDR { get; set; }




        //зависимые свойства
        public virtual bool SelectFromCategories
        {
            get
            {
                bool categoryParameterChecked = CheckCategoryLastSendDate || CheckCategoryEnabled || CheckCategorySendCountNotGreater != null;
                return CategoryID != null && categoryParameterChecked;
            }
        }
        public virtual bool SelectFromTopics
        {
            get
            {
                bool topicParameterChecked = CheckTopicLastSendDate || CheckTopicEnabled || CheckTopicSendCountNotGreater != null;
                return TopicID != null && topicParameterChecked;
            }
        }

    }
}
