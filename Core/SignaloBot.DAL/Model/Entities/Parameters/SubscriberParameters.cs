using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Parameters
{
    public class SubscriberParameters
    {
        //тип доставки, категория, тема
        public int DeliveryType { get; set; }
        public int? CategoryID { get; set; }
        public int? TopicID { get; set; }

        //только пользователи из списка
        public List<Guid> FromUserIDList { get; set; }

        //включать данные при отсутствии категории или темы
        public bool IncludeWithNoCategory { get; set; }
        public bool IncludeWithNoTopic { get; set; }

        //проверять время последней отправки
        public bool CheckTypeLastSendDate { get; set; }
        public bool CheckCategoryLastSendDate { get; set; }
        public bool CheckTopicLastSendDate { get; set; }
        
        //проверять включения (enabled)
        public bool CheckTypeEnabled { get; set; }
        public bool CheckCategoryEnabled { get; set; }
        public bool CheckTopicEnabled { get; set; }

        //проверять лимит на отправку сообщений
        public int? CheckTypeSendCountNotGreater { get; set; }
        public int? CheckCategorySendCountNotGreater { get; set; }
        public int? CheckTopicSendCountNotGreater { get; set; }

        //проверять NDR блок
        public bool CheckBlockedOfNDR { get; set; }




        //зависимые свойства
        public bool SelectFromCategories
        {
            get
            {
                bool categoryParameterChecked = CheckCategoryLastSendDate || CheckCategoryEnabled || CheckCategorySendCountNotGreater != null;
                return CategoryID != null && categoryParameterChecked;
            }
        }
        public bool SelectFromTopics
        {
            get
            {
                bool topicParameterChecked = CheckTopicLastSendDate || CheckTopicEnabled || CheckTopicSendCountNotGreater != null;
                return TopicID != null && topicParameterChecked;
            }
        }

    }
}
