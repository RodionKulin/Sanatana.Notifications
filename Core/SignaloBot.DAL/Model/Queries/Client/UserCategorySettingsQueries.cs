using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Context;
using Common.Utility;
using SignaloBot.DAL.Queries.Client;
using System.Data.SqlClient;
using Common.EntityFramework.SafeCall;
using Common.EntityFramework.Merge;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.DAL.Queries.Client
{
    public class UserCategorySettingsQueries : IUserCategorySettingsQueries 
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<ClientDbContext, UserCategorySettings> _crud;



        //инициализация
        public UserCategorySettingsQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _crud = new EntityCRUD<ClientDbContext, UserCategorySettings>(nameOrConnectionString, prefix);           
        }

        
        //select
        public List<UserCategorySettings> Select(List<Guid> userIDs, int categoryID, out Exception exception)
        {
            List<UserCategorySettings> result = _crud.SelectAll(out exception,
                p => userIDs.Contains(p.UserID)
                && p.CategoryID == categoryID);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }
        

        //insert
        public virtual void Insert(UserCategorySettings settings, out Exception exception)
        {
            _crud.Insert(settings, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void Insert(List<UserCategorySettings> settings, out Exception exception)
        {
            _crud.DbSafeCallAndDispose((context) =>
            {
                context.BulkInsert(settings);
            }, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }        


        //update
        public virtual void UpsertIsEnabled(UserCategorySettings settings, out Exception exception)
        {
            _crud.DbSafeCallAndDispose((context) =>
            {
                MergeOperation<UserCategorySettings> merge = context.Merge(settings);
                
                merge.Compare.IncludeProperty(p => p.UserID)
                    .IncludeProperty(p => p.DeliveryType)
                    .IncludeProperty(p => p.CategoryID);

                merge.Update.IncludeProperty(p => p.IsEnabled);

                merge.Execute(MergeType.Upsert);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }


        //delete
        public virtual void Delete(Guid userID, int deliveryType, int categoryID, out Exception exception)
        {
            UserCategorySettings userCategory = new UserCategorySettings()
            {
                UserID = userID,
                DeliveryType = deliveryType,
                CategoryID = categoryID
            };

            _crud.Delete(userCategory, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void DeleteAll(Guid userID, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
         
            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}UserDeliveryTypeSettings
WHERE UserID = @UserID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        } 
    }
}
