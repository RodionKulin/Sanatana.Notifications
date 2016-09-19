using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using EntityFramework.BulkInsert.Extensions;
using Common.Utility;
using System.Data.SqlClient;
using Common.EntityFramework.Merge;
using Common.EntityFramework;

namespace SignaloBot.DAL.SQL
{
    public class SqlUserCategorySettingsQueries : IUserCategorySettingsQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;



        //инициализация
        public SqlUserCategorySettingsQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }


        //select
        public async Task<QueryResult<List<UserCategorySettings<Guid>>>> Select(List<Guid> userIDs, int categoryID)
        {
            List<UserCategorySettingsGuid> list = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await context.UserCategorySettings.Where(
                        p => userIDs.Contains(p.UserID)
                        && p.CategoryID == categoryID).ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<UserCategorySettingsGuid>();
            
            return new QueryResult<List<UserCategorySettings<Guid>>>(
                list.Cast<UserCategorySettings<Guid>>().ToList(), !result);
        }
        

        //insert
        public virtual Task<bool> Insert(List<UserCategorySettings<Guid>> settings)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.BulkInsert(settings);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return Task.FromResult(result);
        }        


        //update
        public virtual async Task<bool> UpsertIsEnabled(UserCategorySettings<Guid> settings)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    MergeOperation<UserCategorySettings<Guid>> merge = context.Merge(settings);

                    merge.Compare.IncludeProperty(p => p.UserID)
                        .IncludeProperty(p => p.DeliveryType)
                        .IncludeProperty(p => p.CategoryID);

                    merge.Update.IncludeProperty(p => p.IsEnabled);

                    await merge.ExecuteAsync(MergeType.Upsert);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }


        //delete
        public virtual async Task<bool> Delete(Guid userID, int deliveryType, int categoryID)
        {
            UserCategorySettingsGuid userCategory = new UserCategorySettingsGuid()
            {
                UserID = userID,
                DeliveryType = deliveryType,
                CategoryID = categoryID
            };            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.UserCategorySettings.Attach(userCategory);
                    context.Entry(userCategory).State = EntityState.Deleted;
                    await context.SaveChangesAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> Delete(Guid userID)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    string command = string.Format(@"
DELETE {0}UserDeliveryTypeSettings
WHERE UserID = @UserID", _settings.Prefix);

                    int changes = await context.Database.ExecuteSqlCommandAsync(command, userIDParam);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        } 
    }
}
