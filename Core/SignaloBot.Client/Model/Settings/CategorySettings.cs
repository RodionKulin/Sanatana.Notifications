using SignaloBot.Client.DelayScheduler;
using SignaloBot.Client.Templates;
using SignaloBot.DAL;
using SignaloBot.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignaloBot.Client.Settings
{
    public class CategorySettings
    {        
        //свойства
        /// <summary>
        /// Тип доставки сообщений
        /// </summary>
        public int DeliveryType { get; set; }

        /// <summary>
        /// Категория сообщений
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// Номер типа доставки из которого следует получать периоды отправки. Если не указано, то тип доставки сообщений.
        /// </summary>
        public int? ReceivePeriodDeliveryType { get; set; }

        /// <summary>
        /// Номер категории из которой следует получать периоды отправки. Если не указано, то номер категории сообщений.
        /// </summary>
        public int? ReceivePeriodCategoryID { get; set; }

        /// <summary>
        /// Планировщик для определения отложенной даты доставки сообщения. Если не задан используется планировщик по умолчанию из SignaloBotContext.
        /// </summary>
        public IDelayScheduler DelayScheduler { get; set; }

        /// <summary>
        /// Настройки шаблона для составления сообщений.
        /// </summary>
        public SignalTemplate Template { get; set; }



        //инициализация
        public static CategorySettings FromSignalTemplate(SignalTemplate signalTemplate)
        {
            return new CategorySettings()
            {
                CategoryID = signalTemplate.CategoryID,
                DeliveryType = signalTemplate.DeliveryType,
                Template = signalTemplate
            };
        }
    }
}
