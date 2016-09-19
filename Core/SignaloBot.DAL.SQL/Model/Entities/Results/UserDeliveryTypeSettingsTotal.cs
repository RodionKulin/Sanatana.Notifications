using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    internal class UserDeliveryTypeSettingsTotal
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


        //Total
        public long TotalRows { get; set; }



        public static explicit operator UserDeliveryTypeSettings<Guid>(UserDeliveryTypeSettingsTotal t)
        {
            return new UserDeliveryTypeSettings<Guid>()
            {
                UserID = t.UserID,
                DeliveryType = t.DeliveryType,
                Address = t.Address,

                TimeZoneID = t.TimeZoneID,
                LastUserVisitUtc = t.LastUserVisitUtc,                
                LastSendDateUtc = t.LastSendDateUtc,

                IsEnabled = t.IsEnabled,
                IsEnabledOnNewTopic = t.IsEnabledOnNewTopic,

                SendCount = t.SendCount,

                NDRCount = t.NDRCount,
                IsBlockedOfNDR = t.IsBlockedOfNDR,
                BlockOfNDRResetCodeSendDateUtc = t.BlockOfNDRResetCodeSendDateUtc,
                BlockOfNDRResetCode = t.BlockOfNDRResetCode
            };
        }
    }
}
