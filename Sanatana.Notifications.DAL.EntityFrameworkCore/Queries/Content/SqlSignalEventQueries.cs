using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.DAL;
using AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSignalEventQueries : ISignalEventQueries<long>
    {
        //fields        
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;


        //init
        public SqlSignalEventQueries(SqlConnectionSettings connectionSettings, 
            ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory)
        {
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }



        //methods
        public virtual async Task Insert(List<SignalEvent<long>> items)
        {
            List<SignalEventLong> mappedList = items
                .Select(_mapper.Map<SignalEventLong>)
                .ToList();
                
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                var command = repository.Insert<SignalEventLong>();
                command.Insert.ExcludeProperty(x => x.SignalEventId);
                int changes = await command.ExecuteAsync(mappedList).ConfigureAwait(false);
            }
        }

        public virtual async Task<List<SignalEvent<long>>> Find(int count, int maxFailedAttempts)
        {
            RepositoryResult<SignalEventLong> result;

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                result = await repository.FindPageAsync<SignalEventLong, long>(0, count, false,
                    x => x.FailedAttempts < maxFailedAttempts,
                    x => x.SignalEventId,
                    false)
                    .ConfigureAwait(false);                
            }

            return result.Data.Cast<SignalEvent<long>>().ToList();
        }

        public virtual async Task UpdateSendResults(List<SignalEvent<long>> items)
        {
            List<SignalEventLong> mappedList = items
                .Select(_mapper.Map<SignalEventLong>)
                .ToList();

            string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.EVENT_UPDATE_TYPE);
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                MergeCommand<SignalEventLong> update = repository.MergeTVP(mappedList, tvpName);

                update.Source.IncludeProperty(p => p.SignalEventId)
                    .IncludeProperty(p => p.FailedAttempts)
                    .IncludeProperty(p => p.EventSettingsId)
                    .IncludeProperty(p => p.SubscriberIdRangeFrom)
                    .IncludeProperty(p => p.SubscriberIdRangeTo)
                    .IncludeProperty(p => p.SubscriberIdFromDeliveryTypesHandledSerialized);

                update.Compare.IncludeProperty(p => p.SignalEventId);

                update.UpdateMatched
                    .IncludeProperty(p => p.FailedAttempts)
                    .IncludeProperty(p => p.EventSettingsId)
                    .IncludeProperty(p => p.SubscriberIdRangeFrom)
                    .IncludeProperty(p => p.SubscriberIdRangeTo)
                    .IncludeProperty(p => p.SubscriberIdFromDeliveryTypesHandledSerialized);

                int changes = await update.ExecuteAsync(MergeType.Update)
                    .ConfigureAwait(false);               
            }
        }

        public virtual async Task Delete(List<SignalEvent<long>> items)
        {
            List<long> ids = items.Select(p => p.SignalEventId)
                .Distinct()
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SignalEventLong>(
                    x => ids.Contains(x.SignalEventId))
                    .ConfigureAwait(false);
            }
        }
    }
}
