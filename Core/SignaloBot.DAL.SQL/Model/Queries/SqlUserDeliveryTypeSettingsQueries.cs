using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using Common.Utility;
using System.Data.SqlClient;
using System.Data.Entity;
using Common.EntityFramework.Merge;
using EntityFramework.BulkInsert.Extensions;
using Common.EntityFramework;

namespace SignaloBot.DAL.SQL
{
    public class SqlUserDeliveryTypeSettingsQueries : IUserDeliveryTypeSettingsQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;
        

        //инициализация
        public SqlUserDeliveryTypeSettingsQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }


        //insert
        public virtual Task<bool> Insert(List<UserDeliveryTypeSettings<Guid>> settings)
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
        

        //delete
        public virtual async Task<bool> Delete(Guid userID, int deliveryType)
        {
            var settings = new UserDeliveryTypeSettingsGuid()
            {
                UserID = userID,
                DeliveryType = deliveryType
            };            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.UserDeliveryTypeSettings.Attach(settings);
                    context.Entry(settings).State = EntityState.Deleted;
                    int changes = await context.SaveChangesAsync();

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

                    await context.Database.ExecuteSqlCommandAsync(command, userIDParam);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }
        

        //update
        public virtual async Task<bool> Update(UserDeliveryTypeSettings<Guid> settings)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    var settingsGuid = MapperUtility.Mapper.Map<UserDeliveryTypeSettingsGuid>(settings);
                    context.UserDeliveryTypeSettings.Attach(settingsGuid);
                    context.Entry(settings).State = EntityState.Modified;
                    int changes = await context.SaveChangesAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> UpdateAddress(Guid userID, int deliveryType, string address)
        {
            var settings = new UserDeliveryTypeSettingsGuid()
            {
                UserID = userID,
                DeliveryType = deliveryType,
                Address = address
            };            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.UserDeliveryTypeSettings.Attach(settings);
                    DbEntityEntry<UserDeliveryTypeSettingsGuid> entry = context.Entry(settings);
                    entry.Property(p => p.Address).IsModified = true;
                    context.Configuration.ValidateOnSaveEnabled = false;
                    int changes = await context.SaveChangesAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }
        
        public virtual async Task<bool> UpdateLastVisit(Guid userID)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    string command = string.Format(@"
UPDATE {0}UserDeliveryTypeSettings
SET LastUserVisitUtc = GETUTCDATE()
WHERE UserID = @UserID", _settings.Prefix);

                    await context.Database.ExecuteSqlCommandAsync(command, userIDParam);

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> UpdateTimeZone(Guid userID, TimeZoneInfo timeZone)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    SqlParameter timeZoneParam = new SqlParameter("@TimeZoneID", timeZone.Id);
                    string command = string.Format(@"
UPDATE {0}UserDeliveryTypeSettings
SET TimeZoneID = @TimeZoneID
WHERE UserID = @UserID", _settings.Prefix);

                    await context.Database.ExecuteSqlCommandAsync(command, userIDParam, timeZoneParam);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> ResetNDRCount(Guid userID, int deliveryType)
        {
            var settings = new UserDeliveryTypeSettingsGuid()
            {
                UserID = userID,
                DeliveryType = deliveryType,

                NDRCount = 0,
                IsBlockedOfNDR = false,
                BlockOfNDRResetCode = null,
                BlockOfNDRResetCodeSendDateUtc = null
            };
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.UserDeliveryTypeSettings.Attach(settings);
                    DbEntityEntry<UserDeliveryTypeSettingsGuid> entry = context.Entry(settings);
                    entry.Property(p => p.NDRCount).IsModified = true;
                    entry.Property(p => p.IsBlockedOfNDR).IsModified = true;
                    entry.Property(p => p.BlockOfNDRResetCode).IsModified = true;
                    entry.Property(p => p.BlockOfNDRResetCodeSendDateUtc).IsModified = true;

                    context.Configuration.ValidateOnSaveEnabled = false;
                    int changes = await context.SaveChangesAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> UpdateNDRResetCode(Guid userID, int deliveryType, string resetCode)
        {
            var settings = new UserDeliveryTypeSettingsGuid()
            {
                UserID = userID,
                DeliveryType = deliveryType,

                BlockOfNDRResetCode = resetCode,
                BlockOfNDRResetCodeSendDateUtc = DateTime.UtcNow
            };            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.UserDeliveryTypeSettings.Attach(settings);
                    DbEntityEntry<UserDeliveryTypeSettingsGuid> entry = context.Entry(settings);
                    entry.Property(p => p.BlockOfNDRResetCode).IsModified = true;
                    entry.Property(p => p.BlockOfNDRResetCodeSendDateUtc).IsModified = true;

                    context.Configuration.ValidateOnSaveEnabled = false;
                    int changes = await context.SaveChangesAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> DisableAllDeliveryTypes(Guid userID)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    string command = string.Format(@"
UPDATE {0}UserDeliveryTypeSettings
SET IsEnabled = 0
WHERE UserID = @UserID", _settings.Prefix);

                    await context.Database.ExecuteSqlCommandAsync(command, userIDParam);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> UpdateNDRSettings(List<UserDeliveryTypeSettings<Guid>> settings)
        {
            string tvpName = _settings.Prefix + NDR_TVP.USER_NDR_SETTINGS_TYPE;
            bool result = false;

            using (NDRDbContext context = new NDRDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    MergeOperation<UserDeliveryTypeSettings<Guid>> merge = context.Merge(settings, tvpName);

                    merge.Source.IncludeProperty(p => p.UserID)
                        .IncludeProperty(p => p.DeliveryType)
                        .IncludeProperty(p => p.NDRCount)
                        .IncludeProperty(p => p.IsBlockedOfNDR);

                    merge.Compare.IncludeProperty(p => p.UserID)
                        .IncludeProperty(p => p.DeliveryType);

                    merge.Update.IncludeProperty(p => p.NDRCount)
                        .IncludeProperty(p => p.IsBlockedOfNDR);

                    await merge.ExecuteAsync(MergeType.Update);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }



        //select
        public virtual async Task<QueryResult<bool>> CheckAddressExists(int deliveryType, string address)
        {
            bool exist = false;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter addressParam = new SqlParameter("@Address", address);
                    SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", deliveryType);
                    string command = string.Format("EXEC {0}Address_Exists @Address, @DeliveryType", _settings.Prefix);

                    int response = await context.Database.SqlQuery<int>(command
                        , addressParam, deliveryTypeParam)
                        .FirstOrDefaultAsync();

                    exist = response == 1;
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return new QueryResult<bool>(exist, !result);
        }

        public virtual async Task<QueryResult<List<UserDeliveryTypeSettings<Guid>>>> Select(Guid userID)
        {
            List<UserDeliveryTypeSettingsGuid> list = null;            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await context.UserDeliveryTypeSettings
                        .Where(p => p.UserID == userID).ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettingsGuid>();

            return new QueryResult<List<UserDeliveryTypeSettings<Guid>>>(
                list.Cast<UserDeliveryTypeSettings<Guid>>().ToList(), !result);
        }

        public virtual async Task<QueryResult<UserDeliveryTypeSettings<Guid>>> Select(Guid userID, int deliveryType)
        {
            UserDeliveryTypeSettings<Guid> item = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    item = await context.UserDeliveryTypeSettings
                        .Where(p => p.UserID == userID && p.DeliveryType == deliveryType)
                        .FirstOrDefaultAsync();
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return new QueryResult<UserDeliveryTypeSettings<Guid>>(item, !result);
        }

        public virtual async Task<TotalResult<List<UserDeliveryTypeSettings<Guid>>>> SelectPage(int page, int count)
        {
            List<UserDeliveryTypeSettingsTotal> totalList = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    int start;
                    int end;
                    SqlUtility.SqlRowNumber(page, count, out start, out end);

                    SqlParameter firstIndexParam = new SqlParameter("@FirstIndex", start);
                    SqlParameter lastIndexParam = new SqlParameter("@LastIndex", end);

                    string command = string.Format("EXEC {0}UserDeliveryTypeSettings_SelectAll @FirstIndex, @LastIndex"
                        , _settings.Prefix);
                    DbRawSqlQuery<UserDeliveryTypeSettingsTotal> query = context.Database.SqlQuery<UserDeliveryTypeSettingsTotal>(
                        command, firstIndexParam, lastIndexParam);
                    totalList = await query.ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }
            
            if (totalList == null)
                totalList = new List<UserDeliveryTypeSettingsTotal>();

            int total = totalList.Count > 0 ? (int)totalList[0].TotalRows : 0;
            var list = totalList.Select(p => (UserDeliveryTypeSettings<Guid>)p).ToList();

            return new TotalResult<List<UserDeliveryTypeSettings<Guid>>>(list, total, !result);
        }
        
        public virtual async Task<QueryResult<List<UserDeliveryTypeSettings<Guid>>>> Select(
            int deliveryType, List<string> addresses)
        {
            List<UserDeliveryTypeSettingsGuid> list = null;            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await context.UserDeliveryTypeSettings
                        .Where(p => p.DeliveryType == deliveryType && addresses.Contains(p.Address))
                        .ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettingsGuid>();

            return new QueryResult<List<UserDeliveryTypeSettings<Guid>>>(
                list.Cast<UserDeliveryTypeSettings<Guid>>().ToList(), !result);
        }

    }
}
