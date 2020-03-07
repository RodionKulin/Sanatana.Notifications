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
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using System.Linq.Expressions;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSignalBounceQueries : ISignalBounceQueries<long>
    {
        //fields
        protected ISenderDbContextFactory _dbContextFactory;
        
        protected IMapper _mapper;


        //init
        public SqlSignalBounceQueries(ISenderDbContextFactory dbContextFactory
            , INotificationsMapperFactory mapperFactory)
        {
            
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }


        //insert
        public virtual async Task Insert(List<SignalBounce<long>> items)
        {
            List<SignalBounceLong> mappedList = items
                .Select(_mapper.Map<SignalBounceLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<SignalBounceLong> command = repository.Insert<SignalBounceLong>();
                command.Insert.ExcludeProperty(x => x.SignalBounceId);
                int changes = await command.ExecuteAsync(mappedList).ConfigureAwait(false);                
            }
        }


        //select
        public virtual async Task<TotalResult<List<SignalBounce<long>>>> SelectPage(
            int pageIndex, int pageSize, List<long> receiverSubscriberIds = null)
        {
            RepositoryResult<SignalBounceLong> response = null;

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                Expression<Func<SignalBounceLong, bool>> where = x => true;
                if(receiverSubscriberIds != null)
                {
                    where = x => x.ReceiverSubscriberId != null 
                        && receiverSubscriberIds.Contains(x.ReceiverSubscriberId.Value);
                }

                response = await repository.FindPageAsync(pageIndex, pageSize, true,
                        where, x => x.SignalBounceId, true)
                    .ConfigureAwait(false);
            }

            List<SignalBounce<long>> mappedItems = response.Data
                .Select(_mapper.Map<SignalBounce<long>>)
                .ToList();
            return new TotalResult<List<SignalBounce<long>>>(mappedItems, response.TotalRows);
        }


        //delete
        public virtual async Task Delete(List<long> receiverSubscriberIds)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SignalBounceLong>(
                    x => x.ReceiverSubscriberId != null && receiverSubscriberIds.Contains(x.ReceiverSubscriberId.Value))
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task Delete(List<SignalBounce<long>> items)
        {
            List<long> ids = items.Select(p => p.SignalBounceId)
                .Distinct()
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SignalBounceLong>(
                    x => ids.Contains(x.SignalBounceId))
                    .ConfigureAwait(false);
            }
        }
    }
}
