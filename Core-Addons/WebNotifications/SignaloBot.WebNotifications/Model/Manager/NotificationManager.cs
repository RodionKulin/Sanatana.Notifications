using SignaloBot.DAL.Entities;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SignaloBot.WebNotifications.Entities;

namespace SignaloBot.WebNotifications.Manager
{
    public class NotificationManager
    {
        //поля
        NotificationManagerSettings _settings;


        //инициализация
        public NotificationManager(NotificationManagerSettings settings)
        {
            _settings = settings;
        }

        
        
        //получение оповещений
        public List<Notification> SelectAndUpdateLastVisitDate(Guid userID, out int total
            , out Exception exception, CultureInfo culture = null)
        {
            List<Notification> lastNotifies = new List<Notification>();
            exception = null;

            if (!_settings.UseNotificationsPerRequestCaching || HttpContext.Current.Items[NotificationConstans.ITEMS_KEY] == null
                || HttpContext.Current.Items[NotificationConstans.COUNT_KEY] == null)
            {
                int page = 1;
                bool updateVisitDate = true;
                lastNotifies = _settings.NotificationQueries.SelectUpdateLastVisit(userID, updateVisitDate
                    , out total, page, _settings.LastNotificationsCount, out exception);

                CheckUpdates(userID, lastNotifies, out exception, culture);

                if (_settings.UseNotificationsPerRequestCaching)
                {
                    HttpContext.Current.Items[NotificationConstans.ITEMS_KEY] = lastNotifies;
                    HttpContext.Current.Items[NotificationConstans.COUNT_KEY] = total;
                }
            }
            else
            {
                lastNotifies = (List<Notification>)HttpContext.Current.Items[NotificationConstans.ITEMS_KEY];
                total = (int)HttpContext.Current.Items[NotificationConstans.COUNT_KEY];
            }

            return lastNotifies;
        }

        public List<Notification> Select(Guid userID, int page, int itemsCount, out int total, out Exception exception
            , CultureInfo culture = null)
        {
            List<Notification> notifies = _settings.NotificationQueries.SelectUpdateLastVisit(userID, false
                , out total, page, itemsCount, out exception);
            
            CheckUpdates(userID, notifies, out exception, culture);
            
            return notifies;
        }

        private void CheckUpdates(Guid userID, List<Notification> notifies, out Exception exception
            , CultureInfo culture = null)
        {
            exception = null;
            
            //IsDirty
            List<Notification> dirtyNotifies = notifies.Where(p => p.IsDirty).ToList();

            if (dirtyNotifies.Count > 0)
            {
                List<NotificationMeta> metaStrings = _settings.NotificationMetaQueries.Select(
                    userID, dirtyNotifies, out exception);

                RebuildNotifies(dirtyNotifies, metaStrings, culture);

                List<Notification> rebuildedNotifies = dirtyNotifies.Where(p => !p.IsDirty).ToList();

                if (rebuildedNotifies.Count > 0)
                {
                    _settings.NotificationQueries.UpdateDirty(rebuildedNotifies, out exception);
                }
            }
        }
        
        private void RebuildNotifies(List<Notification> notifies, List<NotificationMeta> metaStrings
            , CultureInfo culture = null)
        {
            foreach (Notification notify in notifies)
            {
                NotificationSettings settings = _settings.FindNotificationSettings(notify.CategoryID);

                List<NotificationMeta> notifyMetaStrings = metaStrings.Where(
                    p => p.NotificationID == notify.NotificationID).ToList();

                notify.IsDirty = false;
                notify.NotifyText = settings.BuildNotificationText(notifyMetaStrings, notify.Variant, culture);
            }
        }

        
        
        //удаление
        public void DeleteTopic(Guid userID, int categoryID, int topicID, out Exception exception)
        {
            NotificationSettings settings = _settings.FindNotificationSettings(categoryID);
            
            if (settings.IsUnique)
            {
                DeleteCategory(userID, categoryID, out exception);
            }
            else
            {
                //определяем нужно ли вызывать удаление
                bool deleteRequired = CheckIfDeleteRequired(userID, categoryID
                    , out exception, p => p.TopicID == topicID);

                if (exception != null)
                    return;

                //обновляем базу
                if (deleteRequired)
                {
                    int rowsEffected = _settings.NotificationQueries.DeleteTopic(userID,
                            categoryID, topicID, out exception);

                    if (rowsEffected > 0)
                        ClearHttpContextItems();
                }
            }
        }
        
        public void DeleteTag(Guid userID, int categoryID, string tag, out Exception exception)
        {
            NotificationSettings settings = _settings.FindNotificationSettings(categoryID);

            if (settings.IsUnique)
            {
                DeleteCategory(userID, categoryID, out exception);
            }
            else
            {
                bool deleteRequired = CheckIfDeleteRequired(userID, categoryID, out exception, p => p.Tag == tag);

                if (deleteRequired)
                {
                    int rowsEffected = _settings.NotificationQueries.DeleteTag(userID, categoryID, tag, out exception);
                  
                    if (rowsEffected > 0)
                        ClearHttpContextItems();
                }
            }
        }
        
        public void DeleteCategory(Guid userID, int categoryID, out Exception exception)
        {
            NotificationSettings settings = _settings.FindNotificationSettings(categoryID);
          
            //определяем нужно ли вызывать удаление
            bool deleteRequired = CheckIfDeleteRequired(userID, categoryID, out exception);
           

            //обновляем базу
            if (deleteRequired)
            {
                int rowsEffected = _settings.NotificationQueries.DeleteCategory(userID, categoryID, out exception);
                
                if (rowsEffected > 0)
                    ClearHttpContextItems();
            }
        }
        
        private bool CheckIfDeleteRequired(Guid userID, int categoryID
            , out Exception exception, Func<Notification, bool> selectNotifyExpression = null)
        {
            if (selectNotifyExpression == null)
                selectNotifyExpression = p => true;

            bool deleteRequired = true;
            exception = null;

            if (_settings.UseNotificationsPerRequestCaching)
            {
                int total;
                List<Notification> lastNotifies = SelectAndUpdateLastVisitDate(userID, out total, out exception);

                bool deleteAnyLastNotify = lastNotifies
                    .Where(p => p.CategoryID == categoryID)
                    .Any(selectNotifyExpression);

                if (total <= lastNotifies.Count && !deleteAnyLastNotify)
                    deleteRequired = false;
            }

            return deleteRequired;
        }
        
        private void ClearHttpContextItems()
        {
            HttpContext.Current.Items[NotificationConstans.ITEMS_KEY] = null;
            HttpContext.Current.Items[NotificationConstans.COUNT_KEY] = null;
        }



        //добавление
        public void Insert(Notification notify, Guid receiver, out Exception exception
            , List<NotificationMeta> metaStrings = null)
        {
            Insert(notify, new List<Guid> { receiver }, out exception, metaStrings);
        }
        
        public void Insert(Notification notify, List<Guid> receivers, out Exception exception
            , List<NotificationMeta> metaStrings = null)
        {
            exception = null;         
            if (receivers.Count == 0)
                return;

            NotificationSettings settings = _settings.FindNotificationSettings(notify.CategoryID);

            if (metaStrings == null || !settings.SaveMeta)
                metaStrings = new List<NotificationMeta>();
            
            //добавление в базу
            if (settings.UpsertSameTopic)
            {
                _settings.NotificationQueries.Upsert(notify, metaStrings, receivers, out exception);
            }
            else
            {
                _settings.NotificationQueries.Insert(notify, metaStrings, receivers, out exception);
            }
        }


        //администрирование
        public void InsertNewMetaType(int categoryID, string metaType
            , out Exception exception
            , string defaultMetaKey = null, string defaultMetaValue = null)
        {
            defaultMetaKey = defaultMetaKey ?? string.Empty;
            defaultMetaValue = defaultMetaValue ?? string.Empty;

            _settings.NotificationMetaQueries.InsertNewType(categoryID, metaType,
                defaultMetaKey, defaultMetaValue, out exception);
        }
    }
}
