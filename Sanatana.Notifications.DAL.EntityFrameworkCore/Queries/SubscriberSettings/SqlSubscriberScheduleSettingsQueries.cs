using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Transactions;
using AutoMapper.QueryableExtensions;
using System.Linq.Expressions;
using LinqKit;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore.Commands;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSubscriberScheduleSettingsQueries : ISubscriberScheduleSettingsQueries<long>
    {
        //fields
        protected ISenderDbContextFactory _dbContextFactory;        
        protected IMapper _mapper;


        //init
        public SqlSubscriberScheduleSettingsQueries(ISenderDbContextFactory dbContextFactory
            , INotificationsMapperFactory mapperFactory)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }



        //insert
        public virtual async Task Insert(List<SubscriberScheduleSettings<long>> periods)
        {
            foreach (SubscriberScheduleSettings<long> item in periods)
            {
                item.PeriodBegin = SqlUtility.ToSqlTime(item.PeriodBegin);
                item.PeriodEnd = SqlUtility.ToSqlTime(item.PeriodEnd);
            }
            List<SubscriberScheduleSettingsLong> periodsMapped = periods
                .Select(_mapper.Map<SubscriberScheduleSettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<SubscriberScheduleSettingsLong> command = repository.Insert<SubscriberScheduleSettingsLong>();
                command.Insert.ExcludeProperty(x => x.SubscriberScheduleSettingsId);
                int changes = await command.ExecuteAsync(periodsMapped).ConfigureAwait(false);
            }
        }


        //select
        public virtual async Task<List<SubscriberScheduleSettings<long>>> Select(
            List<long> subscriberIds, List<int> receivePeriodSets = null)
        {
            List<SubscriberScheduleSettingsLong> list = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                IQueryable<SubscriberScheduleSettingsLong> query = context.SubscriberScheduleSettings.Where(
                    p => subscriberIds.Contains(p.SubscriberId));

                if (receivePeriodSets != null)
                {
                    query = query.Where(p => receivePeriodSets.Contains(p.Set));
                }

                list = await query.ToListAsync().ConfigureAwait(false);
            }
            
            return list.Cast<SubscriberScheduleSettings<long>>().ToList();
        }


        //update
        public virtual async Task RewriteSets(long subscriberId, List<SubscriberScheduleSettings<long>> periods)
        {
            foreach (SubscriberScheduleSettings<long> item in periods)
            {
                item.PeriodBegin = SqlUtility.ToSqlTime(item.PeriodBegin);
                item.PeriodEnd = SqlUtility.ToSqlTime(item.PeriodEnd);
            }
            List<int> sets = periods
                .Select(x => x.Set)
                .Distinct()
                .ToList();

            List<SubscriberScheduleSettingsLong> periodsMapped = periods
                .Select(_mapper.Map<SubscriberScheduleSettingsLong>)
                .ToList();

            using (var repository = new Repository(_dbContextFactory.GetDbContext()))
            using (IDbContextTransaction ts = repository.Context.Database.BeginTransaction())
            {
                try
                {
                    SqlTransaction underlyingTransaction = (SqlTransaction)ts.GetDbTransaction();

                    int changes = await repository.DeleteManyAsync<SubscriberScheduleSettingsLong>(
                        x => x.SubscriberId == subscriberId
                        && sets.Contains(x.Set))
                        .ConfigureAwait(false);

                    InsertCommand<SubscriberScheduleSettingsLong> insertCommand = repository
                        .Insert<SubscriberScheduleSettingsLong>(underlyingTransaction);
                    insertCommand.Insert
                        .ExcludeProperty(x => x.SubscriberScheduleSettingsId);
                    changes = await insertCommand.ExecuteAsync(periodsMapped).ConfigureAwait(false);

                    ts.Commit();
                }
                catch (Exception)
                {
                    ts.Rollback();
                    throw;
                }          
            }
        }



        //delete
        public virtual async Task Delete(List<long> subscriberIds, List<int> receivePeriodSets = null)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                Expression<Func<SubscriberScheduleSettingsLong, bool>> filter = x => subscriberIds.Contains(x.SubscriberId);
                if (receivePeriodSets != null)
                {
                    filter = filter.And(x => receivePeriodSets.Contains(x.Set));
                }

                int changes = await repository.DeleteManyAsync<SubscriberScheduleSettingsLong>(filter)
                    .ConfigureAwait(false);
            }
        }

    }
}
