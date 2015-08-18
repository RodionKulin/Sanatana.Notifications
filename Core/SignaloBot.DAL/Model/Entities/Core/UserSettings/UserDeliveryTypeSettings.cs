using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Core
{
    public class UserDeliveryTypeSettings
    {
        //свойства
        public Guid UserID { get; set; }
        public int DeliveryType { get; set; }
        public string Address { get; set; }


        //последнее время посещения и отправки
        public string TimeZoneID { get; set; }
        public DateTime? LastUserVisitUtc { get; set; }
        public DateTime? LastSendDateUtc { get; set; }
        
        //включение
        public bool IsEnabled { get; set; }
        public bool IsEnabledOnNewTopic { get; set; }

        //количество отправленных сообщений
        public int SendCount { get; set; }
        
        //отчёты о недоставленных сообщениях
        public int NDRCount { get; set; }
        public bool IsBlockedOfNDR { get; set; }
        public DateTime? BlockOfNDRResetCodeSendDateUtc { get; set; }
        public string BlockOfNDRResetCode { get; set; }




        //инициализация
        public static UserDeliveryTypeSettings Default(Guid userID, int deliveryType, string address)
        {
            return new UserDeliveryTypeSettings()
            {
                UserID = userID,
                DeliveryType= deliveryType,
                Address = address,
                
                TimeZoneID = null,
                LastUserVisitUtc = null,
                LastSendDateUtc = null,

                IsEnabled = true,
                IsEnabledOnNewTopic = true,

                SendCount = 0,

                NDRCount = 0,
                IsBlockedOfNDR = false,
                BlockOfNDRResetCode = null,
                BlockOfNDRResetCodeSendDateUtc = null
            };
        }
    }
}
