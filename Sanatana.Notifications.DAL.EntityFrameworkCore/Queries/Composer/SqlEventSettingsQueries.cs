using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.Composing;
using Sanatana.Notifications.DAL;
using AutoMapper;
using Sanatana.Notifications.Composing.Templates;
using System.Transactions;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using System.Data.SqlClient;
using Sanatana.EntityFrameworkCore.Commands;
using Sanatana.EntityFrameworkCore.Commands.Merge;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using Microsoft.EntityFrameworkCore.Storage;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlEventSettingsQueries : IEventSettingsQueries<long>
    {
        //fields        
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;
        protected IDispatchTemplateQueries<long> _dispatchTemplateQueries;


        //init
        public SqlEventSettingsQueries(SqlConnectionSettings connectionSettings
            , ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory
            , IDispatchTemplateQueries<long> dispatchTemplateQueries)
        {            
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
            _dispatchTemplateQueries = dispatchTemplateQueries;
        }


        //insert
        public virtual async Task Insert(List<EventSettings<long>> items)
        {
            List<EventSettingsLong> mappedList = items
                .Select(_mapper.Map<EventSettingsLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            using (IDbContextTransaction ts = repository.Context.Database.BeginTransaction())
            {
                try
                {
                    SqlTransaction underlyingTransaction = (SqlTransaction)ts.GetDbTransaction();

                    MergeCommand<EventSettingsLong> mergeCommand = repository.MergeParameters(mappedList, underlyingTransaction);
                    mergeCommand.Insert
                        .ExcludeProperty(x => x.EventSettingsId)
                        .ExcludeProperty(x => x.TemplatesNavigation);
                    mergeCommand.Output
                        .IncludeProperty(x => x.EventSettingsId);
                    int changes = await mergeCommand.ExecuteAsync(MergeType.Insert).ConfigureAwait(false);

                    for (int i = 0; i < mappedList.Count; i++)
                    {
                        EventSettingsLong mappedItem = mappedList[i];
                        items[i].EventSettingsId = mappedItem.EventSettingsId;
                        if (mappedItem.Templates != null)
                        {
                            mappedItem.Templates.ForEach(
                                x => x.EventSettingsId = mappedItem.EventSettingsId);
                        }
                    }

                    List<DispatchTemplate<long>> templates = items
                        .SelectMany(x => x.Templates)
                        .ToList();
                    await InsertTemplates(templates, repository.Context, underlyingTransaction)
                        .ConfigureAwait(false);

                    ts.Commit();
                }
                catch
                {
                    ts.Rollback();
                    throw;
                }
            }
        }

        protected virtual async Task InsertTemplates(List<DispatchTemplate<long>> items
            , DbContext context, SqlTransaction underlyingTransaction)
        {
            List<DispatchTemplateLong> mappedList = items
                .Select(_mapper.Map<DispatchTemplateLong>)
                .ToList();
            
            MergeCommand<DispatchTemplateLong> mergeCommand = new MergeCommand<DispatchTemplateLong>(context, mappedList, underlyingTransaction);
            mergeCommand.Insert
                .ExcludeProperty(x => x.DispatchTemplateId);
            mergeCommand.Output
                .IncludeProperty(x => x.DispatchTemplateId);
            int changes = await mergeCommand.ExecuteAsync(MergeType.Insert).ConfigureAwait(false);
                        
            for (int i = 0; i < mappedList.Count; i++)
            {
                items[i].DispatchTemplateId = mappedList[i].DispatchTemplateId;
            }
        }


        //select
        public virtual async Task<TotalResult<List<EventSettings<long>>>> Select(int page, int pageSize)
        {
            List<EventSettings<long>> list = null;
            int total = 0;

            using (var listDbContext = _dbContextFactory.GetDbContext())
            using (var countDbContext = _dbContextFactory.GetDbContext())
            using (var repository = new Repository(listDbContext))
            {
                IQueryable<EventSettingsLong> listQuery = repository.SelectPageQuery<EventSettingsLong, long>(
                        page, pageSize, true, x => true, x => x.EventSettingsId);

                Task<List<EventSettingsLong>> listTask = listQuery
                    .Include(x => x.TemplatesNavigation)
                    .ToListAsync();
                Task<int> countTask = countDbContext.Set<EventSettingsLong>()
                    .CountAsync();

                List<EventSettingsLong> listLong = await listTask.ConfigureAwait(false);
                total = await countTask.ConfigureAwait(false);

                list = listLong.Cast<EventSettings<long>>().ToList();
            }

            return new TotalResult<List<EventSettings<long>>>(list, total);
        }

        public virtual async Task<EventSettings<long>> Select(long eventSettingsId)
        {
            EventSettingsLong eventSettings = null;

            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                eventSettings = await context.EventSettings
                    .Include(x => x.TemplatesNavigation)
                    .FirstOrDefaultAsync(x => x.EventSettingsId == eventSettingsId)
                    .ConfigureAwait(false);
            }

            return eventSettings;
        }

        public virtual async Task<List<EventSettings<long>>> Select(int category)
        {
            List<EventSettingsLong> eventSettings = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                eventSettings = await context.EventSettings
                        .Where(x => x.CategoryId == category)
                        .Include(x => x.TemplatesNavigation)
                        .ToListAsync()
                        .ConfigureAwait(false);
            }
            
            List<EventSettings<long>> result = eventSettings
                .Cast<EventSettings<long>>()
                .ToList();
            return result;
        }


        //update
        public virtual async Task Update(List<EventSettings<long>> items)
        {
            List<EventSettingsLong> mappedList = items
                .Select(_mapper.Map<EventSettingsLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            using (IDbContextTransaction ts = repository.Context.Database.BeginTransaction())
            {
                try
                {
                    MergeCommand<EventSettingsLong> merge = repository.MergeParameters(mappedList);
                    merge.Compare
                        .IncludeProperty(p => p.EventSettingsId);
                    merge.Update
                        .ExcludeProperty(x => x.EventSettingsId)
                        .ExcludeProperty(x => x.TemplatesNavigation);
                    int changes = await merge.ExecuteAsync(MergeType.Update).ConfigureAwait(false);

                    List<DispatchTemplate<long>> templates = items.SelectMany(x => x.Templates).ToList();
                    await _dispatchTemplateQueries.Update(templates).ConfigureAwait(false);

                    ts.Commit();
                }
                catch(Exception)
                {
                    ts.Rollback();
                    throw;
                }
            }
        }


        //delete
        public virtual async Task Delete(List<EventSettings<long>> items)
        {
            List<long> ids = items.Select(p => p.EventSettingsId)
                .Distinct()
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<EventSettingsLong>(
                    x => ids.Contains(x.EventSettingsId))
                    .ConfigureAwait(false);              
            }
        }

    }
}
