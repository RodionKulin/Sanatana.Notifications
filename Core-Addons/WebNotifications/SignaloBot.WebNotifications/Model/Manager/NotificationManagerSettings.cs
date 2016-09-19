using SignaloBot.Client;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.WebNotifications.Database.Queries;

namespace SignaloBot.WebNotifications.Manager
{
    public class NotificationManagerSettings
    {
        //поля
        int _lastNotificationsCount = 5;
        bool _useNotificationsPerRequestCaching = false;


        //свойства
        /// <summary>
        /// Количество оповещений, которые запрашиваются в методе NotifyManager.GetNotificationsAndUpdateVisitDate.
        /// </summary>
        public int LastNotificationsCount
        {
            get { return _lastNotificationsCount; }
            set { _lastNotificationsCount = value; }
        }
        /// <summary>
        /// Используется ли  HttpContext.Current.Items для хранения результатов NotifyManager.GetNotificationsAndUpdateVisitDate метода и других запросов NotifyManager.
        /// </summary>
        public bool UseNotificationsPerRequestCaching
        {
            get { return _useNotificationsPerRequestCaching; }
            set { _useNotificationsPerRequestCaching = value; }
        }

        public List<NotificationSettings> NotificationSettings { get; set; }

        public ICommonLogger Logger { get; set; }

        public INotificationQueries NotificationQueries { get; set; }

        public INotificationMetaQueries NotificationMetaQueries { get; set; }



        //инициализация
        public NotificationManagerSettings(ICommonLogger logger, string connectionStringOrName, string sqlPrefix = null)
        {
            NotificationQueries = new NotificationQueries(logger, connectionStringOrName, sqlPrefix);
            NotificationMetaQueries = new NotificationMetaQueries(logger, connectionStringOrName, sqlPrefix);
        }


        //методы
        internal NotificationSettings FindNotificationSettings(int categoryID)
        {
            NotificationSettings settings = NotificationSettings.FirstOrDefault(p => p.CategoryID == categoryID);

            if (settings == null)
            {
                string errorMessage = string.Format("Не найдены настройки NotificationSettings с номером категории {0}."
                    , categoryID);

                if (Logger != null)
                    Logger.Error(errorMessage);

                throw new Exception(errorMessage);
            }

            return settings;
        }

    }
}
