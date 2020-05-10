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
using System.Threading;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Queries
{
    public class SqlSignalDispatchHistoryQueries : ISignalDispatchHistoryQueries<long>
    {
        //fields
        protected ISenderDbContextFactory _dbContextFactory;

        protected IMapper _mapper;


        //init
        public SqlSignalDispatchHistoryQueries(ISenderDbContextFactory dbContextFactory
            , INotificationsMapperFactory mapperFactory)
        {

            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }



        //methods
        public Task<long> CountDocuments(Expression<Func<SignalDispatch<long>, bool>> filterConditions, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> DeleteMany(Expression<Func<SignalDispatch<long>, bool>> filterConditions, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<SignalDispatch<long>>> FindMany(Expression<Func<SignalDispatch<long>, bool>> filterConditions, int pageIndex, int pageSize, bool orderDescending = false, Expression<Func<SignalDispatch<long>, object>> orderExpression = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task InsertMany(IEnumerable<SignalDispatch<long>> entities, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> ReplaceMany(IEnumerable<SignalDispatch<long>> entities, bool isUpsert, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

    }
}
