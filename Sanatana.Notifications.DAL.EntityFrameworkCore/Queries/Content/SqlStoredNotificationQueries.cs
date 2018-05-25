using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using AutoMapper;
using Sanatana.Notifications.DAL;
using Sanatana.EntityFrameworkCore.Commands;
using Sanatana.EntityFrameworkCore.Commands.Merge;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlStoredNotificationQueries : IStoredNotificationQueries<long>
    {
        //fields
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;        
        protected IMapper _mapper;



        //init
        public SqlStoredNotificationQueries(ISenderDbContextFactory dbContextFactory
            , INotificationsMapperFactory mapperFactory, SqlConnectionSettings connectionSettings)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
            _connectionSettings = connectionSettings;
        }



        //method
        public virtual async Task Insert(List<StoredNotification<long>> items)
        {
            List<StoredNotificationLong> mappedList = items
                   .Select(_mapper.Map<StoredNotificationLong>)
                   .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<StoredNotificationLong> command = repository.Insert<StoredNotificationLong>();
                command.Insert.ExcludeProperty(x => x.StoredNotificationId);
                int changes = await command.ExecuteAsync(mappedList).ConfigureAwait(false);                
            }
        }

        public virtual async Task<TotalResult<List<StoredNotification<long>>>> Select(List<long> subscriberIds
            , int page, int pageSize, bool descending)
        {
            RepositoryResult<StoredNotificationLong> response = null;

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                response = await repository
                    .SelectPageAsync<StoredNotificationLong, DateTime>(page, pageSize, descending
                    , x => subscriberIds.Contains(x.SubscriberId)
                    , x => x.CreateDateUtc, true)
                    .ConfigureAwait(false);                
            }

            return response.ToTotalResult<StoredNotificationLong, StoredNotification<long>>();
        }

        public virtual async Task Update(List<StoredNotification<long>> items)
        {
            List<StoredNotificationLong> mappedList = items
                .Select(_mapper.Map<StoredNotificationLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema
                    , TableValuedParameters.STORED_NOTIFICATION_TYPE);

                MergeCommand<StoredNotificationLong> merge = repository.MergeTVP(mappedList, tvpName);
                merge.Source
                    .IncludeProperty(x => x.StoredNotificationId)
                    .IncludeProperty(x => x.SubscriberId)
                    .IncludeProperty(x => x.CategoryId)
                    .IncludeProperty(x => x.TopicId)
                    .IncludeProperty(x => x.CreateDateUtc)
                    .IncludeProperty(x => x.MessageSubject)
                    .IncludeProperty(x => x.MessageBody)
                    ;
                merge.Compare
                    .IncludeProperty(p => p.StoredNotificationId);
                merge.Update
                    .ExcludeProperty(p => p.StoredNotificationId);
                int changes = await merge.ExecuteAsync(MergeType.Update).ConfigureAwait(false);
            }
        }

        public virtual async Task Delete(List<StoredNotification<long>> items)
        {
            List<long> ids = items
                .Select(x => x.StoredNotificationId)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<StoredNotificationLong>(
                    x => ids.Contains(x.StoredNotificationId))
                    .ConfigureAwait(false);
            }
        }

    }
}
