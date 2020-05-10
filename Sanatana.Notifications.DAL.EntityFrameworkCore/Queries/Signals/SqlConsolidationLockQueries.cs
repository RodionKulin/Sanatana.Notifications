using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using System.Linq.Expressions;
using System.Threading;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Queries
{
    public class SqlConsolidationLockQueries : IConsolidationLockQueries<long>
    {
        //fields
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;


        //init
        public SqlConsolidationLockQueries(ISenderDbContextFactory dbContextFactory
            , INotificationsMapperFactory mapperFactory)
        {

            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }



        //methods
        public Task Delete(ConsolidationLock<long>[] locksToRemove, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExtendLockTime(ConsolidationLock<long> lockToExtend, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<ConsolidationLock<long>>> FindAll(Expression<Func<ConsolidationLock<long>, bool>> filterConditions = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<ConsolidationLock<long>> FindExistingMatch(ConsolidationLock<long> consolidationLock, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertOneHandleDuplicate(ConsolidationLock<long> entity, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }




    }
}
