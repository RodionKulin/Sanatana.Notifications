using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Queries.Client;
using Common.EntityFramework.Merge;
using Common.EntityFramework.SafeCall;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Entities.Results;


namespace SignaloBot.DAL.Queries.Client
{
    public class UserTopicSettingsQueries : IUserTopicSettingsQueries
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<ClientDbContext, UserTopicSettings> _crud;



        //инициализация
        public UserTopicSettingsQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _crud = new EntityCRUD<ClientDbContext, UserTopicSettings>(nameOrConnectionString, prefix);
        }



        //insert
        public virtual void Upsert(Guid userID, int deliveryType, int categoryID, int topicID
            , bool forceIfIsDeleted, bool? isEnabledOnNewTopic, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", deliveryType);
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", categoryID);
            SqlParameter topicIDParam = new SqlParameter("@TopicID", topicID);
            SqlParameter forceIfDeletedParam = new SqlParameter("@ForceIfDeleted", forceIfIsDeleted);
            SqlParameter isEnabledOnNewTopicParam = new SqlParameter("@IsEnabledOnNewTopic", isEnabledOnNewTopic);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}UserTopicSettings_Insert @UserID, @DeliveryType, @CategoryID, @TopicID, @ForceIfDeleted, @IsEnabledOnNewTopic"
                   , _prefix);
                context.Database.ExecuteSqlCommand(command, userIDParam, deliveryTypeParam,
                    categoryIDParam, topicIDParam, forceIfDeletedParam, isEnabledOnNewTopicParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void UpsertIsEnabled(UserTopicSettings settings, out Exception exception)
        {
            _crud.DbSafeCallAndDispose((context) =>
            {
                MergeOperation<UserTopicSettings> merge = context.Merge(settings);

                merge.Compare.IncludeProperty(p => p.UserID)
                    .IncludeProperty(p => p.DeliveryType)
                    .IncludeProperty(p => p.CategoryID)
                    .IncludeProperty(p => p.TopicID);

                merge.Update.IncludeProperty(p => p.IsEnabled);

                merge.Execute(MergeType.Upsert);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        
        //update
        public virtual void Update(UserTopicSettings settings, out Exception exception)
        {
            _crud.Update(settings, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void MarkDeleted(Guid userID, int deliveryType, int categoryID, int topicID, out Exception exception)
        {
            UserTopicSettings settings = new UserTopicSettings()
            {
                UserID = userID,
                DeliveryType = deliveryType,
                CategoryID = categoryID,
                TopicID = topicID,
                IsDeleted = true
            };

            _crud.Update(settings, out exception, p => p.IsDeleted);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }


        //delete
        public virtual void DeleteAll(Guid userID, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}UserTopicSettings
WHERE UserID = @UserID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
                        

        //select
        public virtual List<UserTopicSettings> SelectPage(Guid userID, int deliveryType, List<int> categoryIDs
            , int start, int end, out int total, out Exception exception)
        {
            List<UserTopicSettingsTotal> list = null;

            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", deliveryType);
            SqlParameter firstIndexParam = new SqlParameter("@FirstIndex", start);
            SqlParameter lastIndexParam = new SqlParameter("@LastIndex", end);
            SqlParameter categoryIDsParam = CoreTVP.ToIntType("@CategoryIDs", categoryIDs, _prefix);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}UserTopicSettings_Select @UserID, @DeliveryType, @FirstIndex, @LastIndex, @CategoryIDs"
                   , _prefix);
                DbRawSqlQuery<UserTopicSettingsTotal> query = context.Database.SqlQuery<UserTopicSettingsTotal>(
                    command, userIDParam, deliveryTypeParam, firstIndexParam, lastIndexParam, categoryIDsParam);
                list = query.ToList();
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserTopicSettingsTotal>();

            total = list.Count > 0 ? (int)list[0].TotalRows : 0;

            return list.Select(p => (UserTopicSettings)p).ToList();
        }

        public virtual UserTopicSettings Find(Guid userID, int deliveryType, int categoryID
            , int topicID, out Exception exception)
        {
            UserTopicSettings result = _crud.SelectFirst(p => p.UserID == userID
                && p.DeliveryType == deliveryType
                && p.CategoryID == categoryID
                && p.TopicID == topicID
                , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }
    }
}
