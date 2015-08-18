using SignaloBot.Client.DelayScheduler;
using SignaloBot.DAL;
using SignaloBot.DAL.Enums;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.Client.Templates;

namespace SignaloBot.Client.Settings
{
    public class SignaloBotContext
    {
        //свойства
        /// <summary>
        /// Логгер исключений
        /// </summary>
        public ICommonLogger Logger { get; set; }

        /// <summary>
        /// Классы доступа к базе
        /// </summary>
        public SignaloBotQueries Queries { get; set; }
        
        /// <summary>
        /// Настройки категорий сообщений
        /// </summary>
        public List<CategorySettings> CategorySettings { get; set; }

        /// <summary>
        /// Планировщик для определения отложенной даты доставки сообщения. Используется по умолчанию, когда в настройках категории не задан свой.
        /// </summary>
        public IDelayScheduler DefaultDelayScheduler { get; set; }


        //инициализация
        public SignaloBotContext(ICommonLogger logger, string connectionStringOrName, string sqlPrefix = null)
        {
            Logger = logger;
            Queries = new SignaloBotQueries(logger, connectionStringOrName, sqlPrefix);
            CategorySettings = new List<CategorySettings>();
            DefaultDelayScheduler = new ReceivePeriodScheduler();
        }
        
        
        //методы
        internal CategorySettings FindCategorySettings(int deliveryType, int categoryID)
        {
            CategorySettings settings = CategorySettings
                .FirstOrDefault(p => p.DeliveryType == deliveryType 
                && p.CategoryID == categoryID);

            if (settings == null)
            {
                string errorMessage = string.Format("Не найдены настройки CategorySettings с типом доставки {0} и номером категории {1}."
                    , deliveryType, categoryID);

                if (Logger != null)
                    Logger.Error(errorMessage);

                throw new Exception(errorMessage);
            }

            return settings;
        }
        
        internal int FindReceivePeriodDeliveryType(int deliveryType, int categoryID)
        {
            CategorySettings settings = FindCategorySettings(deliveryType, categoryID);
            return settings.ReceivePeriodDeliveryType ?? deliveryType;
        }

        internal int FindReceivePeriodCategoryID(int deliveryType, int categoryID)
        {
            CategorySettings settings = FindCategorySettings(deliveryType, categoryID);
            return settings.ReceivePeriodCategoryID ?? categoryID;
        }

        public IDelayScheduler FindDelayScheduler(int deliveryType, int categoryID)
        {
            CategorySettings settings = FindCategorySettings(deliveryType, categoryID);
            IDelayScheduler scheduler = settings.DelayScheduler ?? DefaultDelayScheduler;

            if (scheduler == null)
            {
                string errorMessage = string.Format("Не найден планировщик по умолчанию DefaultDelayScheduler."
                    , categoryID);

                Exception exception = new NullReferenceException(errorMessage);

                if (Logger != null)
                    Logger.Exception(exception);

                throw exception;
            }

            return scheduler;
        }

        public SignalTemplate FindSignalTemplate(int deliveryType, int categoryID)
        {
            CategorySettings settings = CategorySettings
                .FirstOrDefault(p => p.DeliveryType == deliveryType
                && p.CategoryID == categoryID);

            if (settings == null)
            {
                string errorMessage = string.Format("Не найдены настройки CategorySettings с типом доставки {0} и номером категории {1}."
                    , deliveryType, categoryID);

                if (Logger != null)
                {
                    Logger.Error(errorMessage);
                }

                throw new Exception(errorMessage);
            }

            return settings.Template;
        }

    }
}
