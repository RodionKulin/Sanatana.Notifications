using Common.EntityFramework.SafeCall;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.WebNotifications.Entities;

namespace SignaloBot.WebNotifications.Database.Queries
{
    public class NotificationMetaQueries : INotificationMetaQueries
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<NotificationsDbContext, NotificationMeta> _сrud;



        //инициализация
        public NotificationMetaQueries(ICommonLogger logger, string nameOrConnectionString, string prefix = null)
        {
            _сrud = new EntityCRUD<NotificationsDbContext, NotificationMeta>(nameOrConnectionString, prefix);
            _prefix = prefix;
            _logger = logger;
        }

        

        //методы
        public virtual List<NotificationMeta> Select(Guid userID, List<Notification> oldNotifies, out Exception exception)
        {
            List<Guid> notifyIDs = oldNotifies.Select(n => n.NotificationID).ToList();

            List<NotificationMeta> result = _сrud.SelectAll(out exception,
                p => p.UserID == userID &&
                notifyIDs.Contains(p.NotificationID));

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }



        //Notification Meta Admin
        public virtual void InsertNewType(int categoryID, string metaType, string metaKey
            , string metaValue, out Exception exception)
        {
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", categoryID);
            SqlParameter metaTypeParam = new SqlParameter("@MetaType", metaType);
            SqlParameter metaKeyParam = new SqlParameter("@MetaKey", metaKey);
            SqlParameter metaValueParam = new SqlParameter("@MetaValue", metaValue);


            _сrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}NotificationsMeta_InsertNewType @CategoryID, @MetaType, @MetaKey, @MetaValue",
                    _prefix);
                context.Database.ExecuteSqlCommand(command, categoryIDParam, metaTypeParam, metaKeyParam, metaValueParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
    }
}
