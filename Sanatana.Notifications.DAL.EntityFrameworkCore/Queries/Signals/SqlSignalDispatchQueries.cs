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
using Sanatana.Notifications.DAL.Parameters;

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
        protected virtual IQueryable<SignalDispatchLong> Filter(IQueryable<SignalDispatchLong> query, DispatchQueryParameters<long> parameters)
        {
            query = query.Where(msg => msg.SendDateUtc <= DateTime.UtcNow
                && msg.FailedAttempts < parameters.MaxFailedAttempts
                && parameters.ActiveDeliveryTypes.Contains(msg.DeliveryType)
                && !parameters.ExcludeIds.Contains(msg.SignalDispatchId));

            foreach (ConsolidationLock<long> locked in parameters.ExcludeConsolidated)
            {
                query = query.Where(
                   x => x.ReceiverSubscriberId != locked.ReceiverSubscriberId
                   || x.CategoryId != locked.CategoryId
                   || x.DeliveryType != locked.DeliveryType);
            }

            return query;
        }

        public virtual async Task<List<SignalDispatch<long>>> SelectNotSetLock(DispatchQueryParameters<long> parameters)
        {
            List<SignalDispatch<long>> list = null;

            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                IQueryable<SignalDispatchLong> query = context.SignalDispatches.AsQueryable<SignalDispatchLong>();
                query = Filter(query, parameters);
                List<SignalDispatchLong> response = await query
                    .OrderBy(msg => msg.SendDateUtc)
                    .Take(parameters.Count)
                    .ToListAsync()
                    .ConfigureAwait(false);

                list = response
                    .Select(_mapper.Map<SignalDispatch<long>>)
                    .ToList();
            }

            return list;
        }

        public virtual async Task<List<SignalDispatch<long>>> SelectWithSetLock(DispatchQueryParameters<long> parameters, 
            Guid lockId, DateTime lockExpirationDate)
        {
            //same date as stored in LockTracker
            DateTime lockStartTimeUtc = DateTime.UtcNow;

            int lockedDispatches = await SetLock(parameters, lockId,
                lockStartDate: lockStartTimeUtc, lockExpirationDate: lockExpirationDate)
                .ConfigureAwait(false); 
            if (lockedDispatches == 0)
            {
                return new List<SignalDispatch<long>>();
            }

            return await SelectLocked(parameters, lockId, lockExpirationDate)
               .ConfigureAwait(false);
        }

        public virtual async Task<List<SignalDispatch<long>>> SelectLocked(DispatchQueryParameters<long> parameters, 
            Guid lockId, DateTime lockExpirationDate)
        {
            List<SignalDispatch<long>> list = null;

            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                IQueryable<SignalDispatchLong> query = context.SignalDispatches.AsQueryable<SignalDispatchLong>();
                
                query = Filter(query, parameters);
                query = query.Where(msg => msg.LockedBy == lockId
                    && msg.LockedSinceUtc != null
                    && msg.LockedSinceUtc >= lockExpirationDate);

                List<SignalDispatchLong> response = await query
                    .OrderBy(msg => msg.SendDateUtc)
                    .Take(parameters.Count)
                    .ToListAsync()
                    .ConfigureAwait(false);

                list = response
                    .Select(_mapper.Map<SignalDispatch<long>>)
                    .ToList();
            }

            return list;
        }

        public virtual async Task<List<SignalDispatch<long>>> SelectConsolidated(
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
                    && p.CreateDateUtc <= createdBefore);

                if (createdAfter != null)
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
                    .Select(x => new SignalDispatchLong
                    {
                        SignalDispatchId = x.SignalDispatchId,
                        CreateDateUtc = x.CreateDateUtc,
                        TemplateData = x.TemplateData
                    })
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
                    .IncludeProperty(p => p.IsScheduled)
                    .IncludeProperty(p => p.LockedBy)
                    .IncludeProperty(p => p.LockedSinceUtc);

                upsert.Compare.IncludeProperty(p => p.SignalDispatchId);

                upsert.UpdateMatched
                    .IncludeProperty(p => p.SendDateUtc)
                    .IncludeProperty(p => p.FailedAttempts)
                    .IncludeProperty(p => p.IsScheduled)
                    .IncludeProperty(p => p.LockedBy)
                    .IncludeProperty(p => p.LockedSinceUtc);

                int changes = await upsert.ExecuteAsync(MergeType.Update).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> SetLock(List<long> dispatchIds, Guid lockId, DateTime lockStartDate, DateTime lockExpirationDate)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.UpdateMany<SignalDispatchLong>(
                    p => dispatchIds.Contains(p.SignalDispatchId)
                    && (p.LockedBy == null
                    || p.LockedSinceUtc == null
                    || p.LockedSinceUtc < lockExpirationDate))
                    .Assign(x => x.LockedBy, x => lockId)
                    .Assign(x => x.LockedSinceUtc, x => lockStartDate)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return changes == dispatchIds.Count;
            }
        }

        public virtual async Task<int> SetLock(DispatchQueryParameters<long> parameters, Guid lockId, DateTime lockStartDate, DateTime lockExpirationDate)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.UpdateMany<SignalDispatchLong>(
                    p => p.SendDateUtc <= DateTime.UtcNow
                    && p.FailedAttempts < parameters.MaxFailedAttempts
                    && parameters.ActiveDeliveryTypes.Contains(p.DeliveryType)
                    && !parameters.ExcludeIds.Contains(p.SignalDispatchId)
                    && (p.LockedBy == null
                    || p.LockedSinceUtc == null
                    || p.LockedSinceUtc < lockExpirationDate))
                    .Assign(x => x.LockedBy, x => lockId)
                    .Assign(x => x.LockedSinceUtc, x => lockStartDate)
                    .SetLimit(parameters.Count)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return changes;
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

        public virtual Task DeleteConsolidated(List<SignalDispatch<long>> items)
        {
            if (items.Count == 0)
            {
                return Task.CompletedTask;
            }

            var deleteTasks = new List<Task>();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                foreach (SignalDispatch<long> item in items)
                {
                    if (item.ReceiverSubscriberId == null)
                    {
                        continue;
                    }

                    Task<int> deleteTask = repository.DeleteManyAsync<SignalDispatchLong>(
                        p => p.ReceiverSubscriberId == p.ReceiverSubscriberId
                        && p.CategoryId == item.CategoryId
                        && p.DeliveryType == item.DeliveryType
                        && p.CreateDateUtc <= item.SendDateUtc
                        && p.SignalDispatchId != item.SignalDispatchId);
                    deleteTasks.Add(deleteTask);
                }
            }

            return Task.WhenAll(deleteTasks.ToArray());
        }

    }
}
