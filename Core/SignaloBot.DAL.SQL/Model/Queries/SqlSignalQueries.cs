using Common.EntityFramework.Merge;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.Helpers;
using System.Data.Entity;
using System.Linq.Expressions;
using Common.EntityFramework;

namespace SignaloBot.DAL.SQL
{

    public class SqlSignalQueries : ISignalQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;


        //инициализация
        public SqlSignalQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }


        
        //Insert
        public virtual Task<bool> Insert(List<SignalDispatchBase<Guid>> items)
        {
            bool result = false;

            using (SenderDbContext context = new SenderDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.BulkInsert(items);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return Task.FromResult(result);
        }



        //Select
        public virtual async Task<QueryResult<List<SignalDispatchBase<Guid>>>> SelectDelayed(
            Guid userID, List<KeyValuePair<int, int>> deliveryTypeAndCategories)
        {
            if(deliveryTypeAndCategories.Count == 0)
            {
                return new QueryResult<List<SignalDispatchBase<Guid>>>(new List<SignalDispatchBase<Guid>>(), false);
            }

            bool result = false;
            List<SignalDispatchBase<Guid>> list = null;

            using (SenderDbContext context = new SenderDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    IQueryable<SignalDispatchBase<Guid>> request = context.SignalsDispatches.Where(
                        p => p.ReceiverUserID == userID
                        && p.IsDelayed == true);

                    Expression<Func<SignalDispatchBase<Guid>, bool>> categorySelector = deliveryTypeAndCategories.Select(cat => LinqExtensions.ToExpression(
                        exp => exp.DeliveryType == cat.Key
                        && exp.CategoryID == cat.Value))
                        .ToList()
                        .Or();
                    request = request.Where(categorySelector);
                    
                    list = await request.ToListAsync();
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
            {
                list = new List<SignalDispatchBase<Guid>>();
            }

            return new QueryResult<List<SignalDispatchBase<Guid>>>(list, !result);
        }
        
        public virtual async Task<QueryResult<List<SignalDispatchBase<Guid>>>> Select(
            int count, List<int> deliveryTypes, int maxFailedAttempts)
        {
            List<SignalDispatchBaseGuid> list = null;
            bool result = false;

            using (SenderDbContext context = new SenderDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    list = await (from msg in context.SignalsDispatches
                                  orderby msg.SendDateUtc ascending
                                  where msg.SendDateUtc <= DateTime.UtcNow
                                      && deliveryTypes.Contains(msg.DeliveryType)
                                      && msg.FailedAttempts < maxFailedAttempts
                                  select msg)
                            .Take(count).ToListAsync();

                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (list == null)
                list = new List<SignalDispatchBaseGuid>();

            return new QueryResult<List<SignalDispatchBase<Guid>>>(
                list.Cast<SignalDispatchBase<Guid>>().ToList(), !result);
        }

        

        //Update
        public virtual async Task<bool> UpdateSendDateUtc(List<SignalDispatchBase<Guid>> items)
        {
            string tvpName = _settings.Prefix + CoreTVP.SIGNAL_SEND_DATE_TYPE;
            bool result = false;

            using (SenderDbContext context = new SenderDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    MergeOperation<SignalDispatchBase<Guid>> upsert = context.Merge(items, tvpName);

                    upsert.Source.IncludeProperty(p => p.SignalDispatchID)
                        .IncludeProperty(p => p.SendDateUtc)
                        .IncludeProperty(p => p.IsDelayed);

                    upsert.Compare.IncludeProperty(p => p.SignalDispatchID);

                    upsert.Update.IncludeProperty(p => p.SendDateUtc)
                        .IncludeProperty(p => p.IsDelayed);

                    await upsert.ExecuteAsync(MergeType.Update);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }

        public virtual async Task<bool> UpdateCounters(
            UpdateParameters parameters, List<SignalDispatchBase<Guid>> items)
        {
            if (!parameters.UpdateAnything)
            {
                return true;
            }
            
            bool result = false;

            using (SenderDbContext context = new SenderDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    SqlParameter updateUsersParam = CoreTVP.ToUpdateUserType(
                        CoreTVP.UPDATE_USERS_PARAMETER_NAME, items, _settings.Prefix);

                    var scriptCreator = new UpdateCountersQueryCreator();
                    string command = scriptCreator.CreateQuery(parameters, context, _settings.Prefix);
                    
                    await context.Database.ExecuteSqlCommandAsync(command, updateUsersParam);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }
            
        

        //Delete
        public virtual async Task<bool> Delete(List<SignalDispatchBase<Guid>> items)
        {
            bool result = false;

            using (SenderDbContext context = new SenderDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    IEnumerable<Guid> messageIDs = items.Select(p => p.SignalDispatchID).Distinct();
                    string idString = string.Join(",", messageIDs);
                    SqlParameter idsParam = new SqlParameter("@IDs", idString);

                    string command = string.Format(@"
DELETE {0}Signals
WHERE SignalDispatchID IN @IDs", _settings.Prefix);

                    await context.Database.ExecuteSqlCommandAsync(command, idsParam);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return result;
        }



        //IDisposable
        public void Dispose()
        {

        }
    }
}
