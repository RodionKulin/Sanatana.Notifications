using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using System.Data.SqlClient;
using Common.Utility;
using Common.EntityFramework.SafeCall;
using Common.EntityFramework;
using System.Data.Entity;

namespace SignaloBot.DAL.SQL
{
    public class SqlUserReceivePeriodQueries : IUserReceivePeriodQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;



        //инициализация
        public SqlUserReceivePeriodQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }



        //методы
        public virtual async Task<bool> Rewrite(Guid userID, int deliveryType, int categoryID,
            List<UserReceivePeriod<Guid>> periods)
        {
            foreach (UserReceivePeriod<Guid> item in periods)
            {
                item.PeriodBegin = SqlUtility.ToSqlTime(item.PeriodBegin);
                item.PeriodEnd = SqlUtility.ToSqlTime(item.PeriodEnd);
            }
            
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", deliveryType);
                    SqlParameter categoryIDParam = new SqlParameter("@categoryID", categoryID);
                    SqlParameter periodsParam = CoreTVP.ToUserReceivePeriodType("@Periods", periods, _settings.Prefix);

                    string command = string.Format("EXEC {0}UserReceivePeriods_Rewrite @UserID, @DeliveryType, @categoryID, @Periods",
                       _settings.Prefix);
                    await context.Database.ExecuteSqlCommandAsync(command, userIDParam
                        , deliveryTypeParam, categoryIDParam, periodsParam);

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual Task<bool> Insert(List<UserReceivePeriod<Guid>> periods)
        {
            bool result = false;
            foreach (UserReceivePeriod<Guid> item in periods)
            {
                item.PeriodBegin = SqlUtility.ToSqlTime(item.PeriodBegin);
                item.PeriodEnd = SqlUtility.ToSqlTime(item.PeriodEnd);
            }

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.BulkInsert(periods);                  
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
        public virtual async Task<bool> Delete(UserReceivePeriod<Guid> period)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    var periodGuid = MapperUtility.Mapper.Map<UserReceivePeriodGuid>(period);
                    context.UserReceivePeriods.Attach(periodGuid);
                    context.Entry(period).State = EntityState.Deleted;
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
DELETE {0}UserReceivePeriods
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

        public virtual async Task<bool> Delete(Guid userID, int deliveryType, int categoryID)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", deliveryType);
                    SqlParameter categoryIDParam = new SqlParameter("@CategoryID", categoryID);

                    string command = string.Format(@"
DELETE {0}UserReceivePeriods
WHERE UserID = @UserID
AND DeliveryType = @DeliveryType
AND CategoryID = @CategoryID", _settings.Prefix);

                    await context.Database.ExecuteSqlCommandAsync(command, userIDParam, deliveryTypeParam, categoryIDParam);
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
        public virtual async Task<bool> Update(UserReceivePeriod<Guid> period)
        {
            bool result = false;
            period.PeriodBegin = SqlUtility.ToSqlTime(period.PeriodBegin);
            period.PeriodEnd = SqlUtility.ToSqlTime(period.PeriodEnd);
            
            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    var periodGuid = MapperUtility.Mapper.Map<UserReceivePeriodGuid>(period);
                    context.UserReceivePeriods.Attach(periodGuid);
                    context.Entry(period).State = EntityState.Modified;
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



        //select
        public virtual async Task<QueryResult<List<UserReceivePeriod<Guid>>>> Select(Guid userID)
        {
            List<UserReceivePeriodGuid> list = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await context.UserReceivePeriods
                        .Where(p => p.UserID == userID).ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<UserReceivePeriodGuid>();

            return new QueryResult<List<UserReceivePeriod<Guid>>>(
                    list.Cast<UserReceivePeriod<Guid>>().ToList(), !result);
        }

        public virtual async Task<QueryResult<List<UserReceivePeriod<Guid>>>> Select(Guid userID
            , int deliveryType, int categoryID)
        {
            List<UserReceivePeriodGuid> list = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await context.UserReceivePeriods.Where(
                        p => p.UserID == userID
                        && p.CategoryID == categoryID
                        && p.DeliveryType == deliveryType)
                        .ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<UserReceivePeriodGuid>();

            return new QueryResult<List<UserReceivePeriod<Guid>>>(
                    list.Cast<UserReceivePeriod<Guid>>().ToList(), !result);
        }

        public virtual async Task<QueryResult<List<UserReceivePeriod<Guid>>>> Select(
            List<Guid> userIDs, int deliveryType, int categoryID)
        {
            List<UserReceivePeriodGuid> list = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await context.UserReceivePeriods.Where(
                        p => p.CategoryID == categoryID
                        && p.DeliveryType == deliveryType
                        && userIDs.Contains(p.UserID))
                        .ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<UserReceivePeriodGuid>();

            return new QueryResult<List<UserReceivePeriod<Guid>>>(
                    list.Cast<UserReceivePeriod<Guid>>().ToList(), !result);
        }

    }
}
