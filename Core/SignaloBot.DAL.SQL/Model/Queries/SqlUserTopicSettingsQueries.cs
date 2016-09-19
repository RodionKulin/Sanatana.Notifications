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
using SignaloBot.DAL.SQL;
using System.Data.Entity;
using Common.EntityFramework;
using EntityFramework.BulkInsert.Extensions;

namespace SignaloBot.DAL.SQL
{
    public class SqlUserTopicSettingsQueries : IUserTopicSettingsQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;



        //инициализация
        public SqlUserTopicSettingsQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }



        //insert
        public virtual Task<bool> Insert(List<UserTopicSettings<Guid>> settings)
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



        //select
        public virtual async Task<TotalResult<List<UserTopicSettings<Guid>>>> SelectPage(
            Guid userID, List<int> categoryIDs, int page, int count)
        {
            List<UserTopicSettingsTotal> totalList = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    int start;
                    int end;
                    SqlUtility.SqlRowNumber(page, count, out start, out end);

                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);
                    SqlParameter firstIndexParam = new SqlParameter("@FirstIndex", start);
                    SqlParameter lastIndexParam = new SqlParameter("@LastIndex", end);
                    SqlParameter categoryIDsParam = CoreTVP.ToIntType("@CategoryIDs", categoryIDs, _settings.Prefix);

                    string command = string.Format("EXEC {0}UserTopicSettings_Select @UserID, @DeliveryType, @FirstIndex, @LastIndex, @CategoryIDs"
                        , _settings.Prefix);
                    DbRawSqlQuery<UserTopicSettingsTotal> query = context.Database.SqlQuery<UserTopicSettingsTotal>(
                        command, userIDParam, firstIndexParam, lastIndexParam, categoryIDsParam);

                    totalList = await query.ToListAsync();
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }
            
            if (totalList == null)
                totalList = new List<UserTopicSettingsTotal>();

            int total = totalList.Count > 0 ? (int)totalList[0].TotalRows : 0;
            var list = totalList.Select(p => (UserTopicSettings<Guid>)p).ToList();

            return new TotalResult<List<UserTopicSettings<Guid>>>(list, total, !result);
        }

        public virtual async Task<QueryResult<UserTopicSettings<Guid>>> Select(
            Guid userID, int categoryID, string topicID)
        {
            UserTopicSettings<Guid> item = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    item = await context.UserTopicSettings.Where(
                        p => p.UserID == userID
                        && p.CategoryID == categoryID
                        && p.TopicID == topicID)
                        .FirstOrDefaultAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return new QueryResult<UserTopicSettings<Guid>>(item, !result);
        }



        //update
        public virtual async Task<bool> Upsert(UserTopicSettings<Guid> settings, bool updateExisting)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    MergeOperation<UserTopicSettings<Guid>> merge = context.Merge(settings);

                    merge.Compare.IncludeProperty(p => p.UserID)
                        .IncludeProperty(p => p.CategoryID)
                        .IncludeProperty(p => p.TopicID);

                    if (updateExisting)
                    {
                        merge.Update.IncludeProperty(p => p.IsEnabled)
                            .IncludeProperty(p => p.IsDeleted);
                    }

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

        public virtual async Task<bool> UpdateIsDeleted(UserTopicSettings<Guid> settings)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    var settingsGuid = MapperUtility.Mapper.Map<UserTopicSettingsGuid>(settings);
                    context.UserTopicSettings.Attach(settingsGuid);
                    DbEntityEntry<UserTopicSettings<Guid>> entry = context.Entry(settings);
                    entry.Property(p => p.IsDeleted).IsModified = true;

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



        //delete
        public virtual async Task<bool> Delete(Guid userID)
        {
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter userIDParam = new SqlParameter("@UserID", userID);

                    string command = string.Format(@"
DELETE {0}UserTopicSettings
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

        public virtual async Task<bool> Delete(Guid userID, int categoryID, string topicID)
        {
            var settings = new UserTopicSettingsGuid()
            {
                UserID = userID,
                CategoryID = categoryID,
                TopicID = topicID
            };
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.UserTopicSettings.Attach(settings);
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

    }
}
