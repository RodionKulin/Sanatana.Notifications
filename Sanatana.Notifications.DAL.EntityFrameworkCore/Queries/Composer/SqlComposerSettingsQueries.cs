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
    public class SqlComposerSettingsQueries : IComposerSettingsQueries<long>
    {
        //fields        
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;
        protected IDispatchTemplateQueries<long> _dispatchTemplateQueries;


        //init
        public SqlComposerSettingsQueries(SqlConnectionSettings connectionSettings
            , ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory
            , IDispatchTemplateQueries<long> dispatchTemplateQueries)
        {            
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
            _dispatchTemplateQueries = dispatchTemplateQueries;
        }


        //insert
        public virtual async Task Insert(List<ComposerSettings<long>> items)
        {
            List<ComposerSettingsLong> mappedList = items
                .Select(_mapper.Map<ComposerSettingsLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            using (IDbContextTransaction ts = repository.Context.Database.BeginTransaction())
            {
                try
                {
                    SqlTransaction underlyingTransaction = (SqlTransaction)ts.GetDbTransaction();

                    MergeCommand<ComposerSettingsLong> mergeCommand = repository.MergeParameters(mappedList, underlyingTransaction);
                    mergeCommand.Insert
                        .ExcludeProperty(x => x.ComposerSettingsId)
                        .ExcludeProperty(x => x.TemplatesNavigation);
                    mergeCommand.Output
                        .IncludeProperty(x => x.ComposerSettingsId);
                    int changes = await mergeCommand.ExecuteAsync(MergeType.Insert).ConfigureAwait(false);

                    for (int i = 0; i < mappedList.Count; i++)
                    {
                        ComposerSettingsLong mappedItem = mappedList[i];
                        items[i].ComposerSettingsId = mappedItem.ComposerSettingsId;
                        if (mappedItem.Templates != null)
                        {
                            mappedItem.Templates.ForEach(
                                x => x.ComposerSettingsId = mappedItem.ComposerSettingsId);
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
        public virtual async Task<TotalResult<List<ComposerSettings<long>>>> Select(int page, int pageSize)
        {
            List<ComposerSettings<long>> list = null;
            int total = 0;

            using (var listDbContext = _dbContextFactory.GetDbContext())
            using (var countDbContext = _dbContextFactory.GetDbContext())
            using (var repository = new Repository(listDbContext))
            {
                IQueryable<ComposerSettingsLong> listQuery = repository.SelectPageQuery<ComposerSettingsLong, long>(
                        page, pageSize, true, x => true, x => x.ComposerSettingsId);

                Task<List<ComposerSettingsLong>> listTask = listQuery
                    .Include(x => x.TemplatesNavigation)
                    .ToListAsync();
                Task<int> countTask = countDbContext.Set<ComposerSettingsLong>()
                    .CountAsync();

                List<ComposerSettingsLong> listLong = await listTask.ConfigureAwait(false);
                total = await countTask.ConfigureAwait(false);

                list = listLong.Cast<ComposerSettings<long>>().ToList();
            }

            return new TotalResult<List<ComposerSettings<long>>>(list, total);
        }

        public virtual async Task<ComposerSettings<long>> Select(long composerSettingsId)
        {
            ComposerSettingsLong composerSettings = null;

            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                composerSettings = await context.ComposerSettings
                    .Include(x => x.TemplatesNavigation)
                    .FirstOrDefaultAsync(x => x.ComposerSettingsId == composerSettingsId)
                    .ConfigureAwait(false);
            }

            return composerSettings;
        }

        public virtual async Task<List<ComposerSettings<long>>> Select(int category)
        {
            List<ComposerSettingsLong> composerSettings = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                composerSettings = await context.ComposerSettings
                        .Where(x => x.CategoryId == category)
                        .Include(x => x.TemplatesNavigation)
                        .ToListAsync()
                        .ConfigureAwait(false);
            }
            
            List<ComposerSettings<long>> result = composerSettings
                .Cast<ComposerSettings<long>>()
                .ToList();
            return result;
        }


        //update
        public virtual async Task Update(List<ComposerSettings<long>> items)
        {
            List<ComposerSettingsLong> mappedList = items
                .Select(_mapper.Map<ComposerSettingsLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            using (IDbContextTransaction ts = repository.Context.Database.BeginTransaction())
            {
                try
                {
                    string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.COMPOSER_SETTINGS_TYPE);
                    MergeCommand<ComposerSettingsLong> merge = repository.MergeTVP(mappedList, tvpName);
                    merge.Compare.IncludeProperty(p => p.ComposerSettingsId);
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
        public virtual async Task Delete(List<ComposerSettings<long>> items)
        {
            List<long> ids = items.Select(p => p.ComposerSettingsId)
                .Distinct()
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<ComposerSettingsLong>(
                    x => ids.Contains(x.ComposerSettingsId))
                    .ConfigureAwait(false);              
            }
        }

    }
}
