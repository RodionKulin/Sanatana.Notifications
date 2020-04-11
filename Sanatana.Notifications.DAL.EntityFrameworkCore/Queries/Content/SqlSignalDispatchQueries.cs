using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using System.Reflection;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Queries
{
    public class SqlSignalDispatchQueries : ISignalDispatchQueries<long>
    {
        //fields        
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;


        //init
        public SqlSignalDispatchQueries(SqlConnectionSettings connectionSettings
            , ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory)
        {
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }


        //Insert
        public virtual async Task Insert(List<SignalDispatch<long>> items)
        {
            List<SignalDispatchLong> mappedList = items
                .Select(_mapper.Map<SignalDispatchLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                var command = repository.Insert<SignalDispatchLong>();
                command.Insert.ExcludeProperty(x => x.SignalDispatchId);
                int changes = await command.ExecuteAsync(mappedList).ConfigureAwait(false);               
            }
        }


        //Select
        public virtual async Task<List<SignalDispatch<long>>> SelectCreatedBefore(
            int pageSize, List<long> subscriberIds, List<(int deliveryType, int category)> categories, 
            DateTime createdBefore, DateTime? createdAfter = null)
        {
            if (categories.Count == 0)
            {
                return new List<SignalDispatch<long>>();
            }

            List<SignalDispatch<long>> list = null;

            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                IQueryable<SignalDispatchLong> request = context.SignalDispatches.Where(
                    p => p.ReceiverSubscriberId != null
                    && subscriberIds.Contains(p.ReceiverSubscriberId.Value)
                    && p.CreateDateUtc < createdBefore);

                if(createdAfter != null)
                {
                    request = request.Where(x => createdAfter.Value < x.CreateDateUtc);
                }

                Expression<Func<SignalDispatchLong, bool>> categorySelector = categories
                    .Select<(int deliveryType, int category), Expression<Func<SignalDispatchLong, bool>>>(cat =>
                        exp => exp.DeliveryType == cat.deliveryType
                        && exp.CategoryId == cat.category)
                    .Or();
                request = request.Where(categorySelector);

                List<SignalDispatchLong> response = await request
                    .OrderBy(x => x.CreateDateUtc)
                    .Take(pageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);
                list = response
                    .Select(_mapper.Map<SignalDispatch<long>>)
                    .ToList();
            }

            return list;
        }

        public virtual async Task<List<SignalDispatch<long>>> Select(
            int count, List<int> deliveryTypes, int maxFailedAttempts)
        {
            List<SignalDispatch<long>> list = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                List<SignalDispatchLong> response = await 
                    (from msg in context.SignalDispatches
                    orderby msg.SendDateUtc ascending
                    where msg.SendDateUtc <= DateTime.UtcNow
                        && msg.FailedAttempts < maxFailedAttempts
                        && deliveryTypes.Contains(msg.DeliveryType)
                    select msg)
                    .Take(count)
                    .ToListAsync()
                    .ConfigureAwait(false);

                list = response
                    .Select(_mapper.Map<SignalDispatch<long>>)
                    .ToList();
            }
            
            return list;
        }


        //Update
        public virtual async Task UpdateSendResults(List<SignalDispatch<long>> items)
        {
            List<SignalDispatchLong> mappedList = items
                .Select(_mapper.Map<SignalDispatchLong>)
                .ToList();

            string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.DISPATCH_UPDATE_TYPE);
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                MergeCommand<SignalDispatchLong> upsert = repository.MergeTVP(mappedList, tvpName);

                upsert.Source.IncludeProperty(p => p.SignalDispatchId)
                    .IncludeProperty(p => p.SendDateUtc)
                    .IncludeProperty(p => p.FailedAttempts)
                    .IncludeProperty(p => p.IsScheduled);

                upsert.Compare.IncludeProperty(p => p.SignalDispatchId);

                upsert.UpdateMatched
                    .IncludeProperty(p => p.SendDateUtc)
                    .IncludeProperty(p => p.FailedAttempts)
                    .IncludeProperty(p => p.IsScheduled);

                int changes = await upsert.ExecuteAsync(MergeType.Update).ConfigureAwait(false);
            }
        }


        //Delete
        public virtual async Task Delete(List<SignalDispatch<long>> items)
        {
            List<long> ids = items.Select(p => p.SignalDispatchId)
                .Distinct()
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SignalDispatchLong>(
                    x => ids.Contains(x.SignalDispatchId))
                    .ConfigureAwait(false);                
            }
        }

    }
}
