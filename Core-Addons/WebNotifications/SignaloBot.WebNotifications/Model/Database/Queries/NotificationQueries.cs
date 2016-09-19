using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.Entity.Infrastructure;
using Common.Utility;
using Common.EntityFramework.SafeCall;
using Common.EntityFramework.Merge;
using Common.EntityFramework;
using SignaloBot.WebNotifications.Entities.Results;
using SignaloBot.WebNotifications.Entities;
using SignaloBot.DAL;
using SignaloBot.DAL.SQL;

namespace SignaloBot.WebNotifications.Database.Queries
{
    public class NotificationQueries : INotificationQueries
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<NotificationsDbContext, Notification> _сrud;



        //инициализация
        public NotificationQueries(ICommonLogger logger, string nameOrConnectionString, string prefix = null)
        {
            _сrud = new EntityCRUD<NotificationsDbContext, Notification>(nameOrConnectionString, prefix);
            _prefix = prefix;
            _logger = logger;
        }
                


        //Insert
        public virtual void Insert(Notification notification, List<NotificationMeta> notifyMetas,
            List<Guid> userIDs, out Exception exception)
        {
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", notification.CategoryID);
            SqlParameter topicIDParam = new SqlParameter("@TopicID", notification.TopicID);
            SqlParameter notifyTextParam = new SqlParameter("@NotifyText", notification.NotifyText);
            SqlParameter tagParam = new SqlParameter("@Tag", notification.Tag);
            SqlParameter variantParam = new SqlParameter("@Variant", notification.Variant);
            SqlParameter cultureParam = new SqlParameter("@Culture", notification.Culture);
            SqlParameter notifyMetasParam = NotificationsTVP.ToNotificationMetaType("@NotifyMetas", notifyMetas, _prefix);
            SqlParameter userIDParam = CoreTVP.ToGuidType("@UserIDs", userIDs, _prefix);
            

            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}Notifications_Insert @CategoryID, @TopicID, @NotifyText, @Tag, @Variant, @Culture, @NotifyMetas, @UserIDs",
                    _prefix);
                context.Database.ExecuteSqlCommand(command, categoryIDParam, topicIDParam, notifyTextParam, tagParam
                    , variantParam, cultureParam, notifyMetasParam, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void Upsert(Notification notification, List<NotificationMeta> notifyMetas,
            List<Guid> userIDs, out Exception exception)
        {
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", notification.CategoryID);
            SqlParameter topicIDParam = new SqlParameter("@TopicID", notification.TopicID);
            SqlParameter notifyTextParam = new SqlParameter("@NotifyText", notification.NotifyText);
            SqlParameter tagParam = new SqlParameter("@Tag", notification.Tag);
            SqlParameter variantParam = new SqlParameter("@Variant", notification.Variant);
            SqlParameter cultureParam = new SqlParameter("@Culture", notification.Culture);
            SqlParameter notifyMetasParam = NotificationsTVP.ToNotificationMetaType("@NotifyMetas", notifyMetas, _prefix);
            SqlParameter userIDParam = CoreTVP.ToGuidType("@UserIDs", userIDs, _prefix);

            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}Notifications_Upsert @CategoryID, @TopicID, @NotifyText, @Tag, @Variant, @Culture, @NotifyMetas, @UserIDs",
                    _prefix);
                context.Database.ExecuteSqlCommand(command, categoryIDParam, topicIDParam, notifyTextParam
                    , tagParam, variantParam, cultureParam, notifyMetasParam, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
        

        //Update
        /// <summary>
        /// Обновляет текст оповещения, если установлена метка IsDirty.
        /// </summary>
        /// <param name="notifies"></param>
        /// <param name="exception"></param>
        public virtual void UpdateDirty(List<Notification> notifies, out Exception exception)
        {
            string tvpName = _prefix + NotificationsTVP.NOTIFICATION_TYPE;

            _сrud.DbSafeCallAndDispose((context) =>
            {
                MergeOperation<Notification> upsert = context.Merge<Notification>(notifies, tvpName);

                upsert.Source.IncludeProperty(p => p.NotificationID)
                    .IncludeProperty(p => p.NotifyText)
                    .IncludeProperty(p => p.IsDirty);

                upsert.Compare.IncludeProperty(p => p.NotificationID);

                upsert.Update.IncludeProperty(p => p.NotifyText)
                    .IncludeProperty(p => p.IsDirty);

                upsert.Execute(MergeType.Update);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
        

        //Select
        public virtual List<Notification> SelectUpdateLastVisit(Guid userID, bool updateVisit,
            out int total, int page, int itemsOnPage, out Exception exception)
        {
            List<NotificationTotal> notifies = null;

            int firstIndex, lastIndex;
            SqlUtility.SqlRowNumber(page, itemsOnPage, out firstIndex, out lastIndex);

            SqlParameter userIdParam = new SqlParameter("@UserID", userID);
            SqlParameter firstIndexParam = new SqlParameter("FirstIndex", firstIndex);
            SqlParameter lastIndexParam = new SqlParameter("@LastIndex", lastIndex);
            SqlParameter updateVisitParam = new SqlParameter("@UpdateVisit", updateVisit);

            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}Notifications_SelectUpdateLastVisit @UserId, @FirstIndex, @LastIndex, @UpdateVisit",
                    _prefix);  
               
                DbRawSqlQuery<NotificationTotal> query = context.Database.SqlQuery<NotificationTotal>(
                    command, userIdParam, firstIndexParam, lastIndexParam, updateVisitParam);

                notifies = query.ToList();
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
            
            if (notifies == null)
                notifies = new List<NotificationTotal>();

            total = notifies.Count > 0 ? (int)notifies[0].TotalRows : 0;


            return notifies.Select(p => (Notification)p).ToList();
        }
        

        //Delete
        public virtual int DeleteCategory(Guid userID, int categoryID, out Exception exception)
        {
            int rowsEffected = 0;
            
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", categoryID);

            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}Notifications
WHERE UserID = @UserID
AND CategoryID = @CategoryID", _prefix);

                rowsEffected = context.Database.ExecuteSqlCommand(command, userIDParam, categoryIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return rowsEffected;
        }

        public virtual int DeleteTopic(Guid userID, int categoryID, string topicID, out Exception exception)
        {
            int rowsEffected = 0;

            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", categoryID);
            SqlParameter topicIDParam = new SqlParameter("@TopicID", topicID);

            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}Notifications
WHERE UserID = @UserID
AND CategoryID = @CategoryID
AND TopicID = @TopicID", _prefix);

                rowsEffected = context.Database.ExecuteSqlCommand(command, userIDParam, categoryIDParam, topicIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return rowsEffected;
        }

        public virtual int DeleteTag(Guid userID, int categoryID, string tag, out Exception exception)
        {
            int rowsEffected = 0;

            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", categoryID);
            SqlParameter tagParam = new SqlParameter("@Tag", tag);

            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}Notifications
WHERE UserID = @UserID
AND CategoryID = @CategoryID
AND Tag = @Tag", _prefix);

                rowsEffected = context.Database.ExecuteSqlCommand(command, userIDParam, categoryIDParam, tagParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return rowsEffected;
        }
    }
}
