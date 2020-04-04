using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Queries
{
    public class SqlEventSettingsQueries : IEventSettingsQueries<long>
    {
        //fields        
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;
        protected IDispatchTemplateQueries<long> _dispatchTemplateQueries;


        //init
        public SqlEventSettingsQueries(ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory
            , IDispatchTemplateQueries<long> dispatchTemplateQueries)
        {            
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
                SqlTransaction underlyingTransaction = (SqlTransaction)ts.GetDbTransaction();

                MergeCommand<EventSettingsLong> mergeCommand = repository.Merge(mappedList, underlyingTransaction);
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
        }

        protected virtual async Task InsertTemplates(List<DispatchTemplate<long>> items
            , DbContext context, SqlTransaction underlyingTransaction)
        {
            List<DispatchTemplateLong> mappedList = items
                .Select(_mapper.Map<DispatchTemplateLong>)
                .ToList();
            
            var mergeCommand = new MergeCommand<DispatchTemplateLong>(context, mappedList, underlyingTransaction);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<EventSettings<long>>>> Select(int pageIndex, int pageSize)
        {
            List<EventSettings<long>> list = null;
            int total = 0;

            using (var listDbContext = _dbContextFactory.GetDbContext())
            using (var countDbContext = _dbContextFactory.GetDbContext())
            using (var repository = new Repository(listDbContext))
            {
                IQueryable<EventSettingsLong> listQuery = repository.FindPageQuery<EventSettingsLong, long>(
                        pageIndex, pageSize, true, x => true, x => x.EventSettingsId);

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

        public virtual async Task<List<EventSettings<long>>> SelectByKey(int eventKey)
        {
            List<EventSettingsLong> eventSettings = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                eventSettings = await context.EventSettings
                        .Where(x => x.EventKey == eventKey)
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
                SqlTransaction underlyingTransaction = (SqlTransaction)ts.GetDbTransaction();

                MergeCommand<EventSettingsLong> merge = repository.Merge(mappedList, underlyingTransaction);
                merge.Compare
                    .IncludeProperty(p => p.EventSettingsId);
                merge.UpdateMatched
                    .ExcludeProperty(x => x.EventSettingsId);
                int changes = await merge.ExecuteAsync(MergeType.Update).ConfigureAwait(false);

                List<DispatchTemplate<long>> templates = items.SelectMany(x => x.Templates).ToList();
                await UpdateTemplates(templates, repository.Context, underlyingTransaction).ConfigureAwait(false);

                ts.Commit();
            }
        }
        
        public virtual async Task UpdateTemplates(List<DispatchTemplate<long>> items, DbContext context, SqlTransaction transaction)
        {
            List<DispatchTemplateLong> mappedItems = items
                .Select(_mapper.Map<DispatchTemplateLong>)
                .ToList();

            MergeCommand<DispatchTemplateLong> merge = new MergeCommand<DispatchTemplateLong>(context, mappedItems, transaction);
            merge.Compare
                .IncludeProperty(p => p.DispatchTemplateId);
            merge.UpdateMatched
                .ExcludeProperty(x => x.DispatchTemplateId);
            int changes = await merge.ExecuteAsync(MergeType.Update).ConfigureAwait(false);
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
