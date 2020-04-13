using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.EventsHandling.Templates;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sanatana.Notifications.DAL;
using AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Queries
{
    public class SqlDispatchTemplateQueries : IDispatchTemplateQueries<long>
    {
        //fields        
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;


        //init
        public SqlDispatchTemplateQueries(ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }
        

        //insert
        public virtual async Task Insert(List<DispatchTemplate<long>> items)
        {
            List<DispatchTemplateLong> mappedList = items
                .Select(_mapper.Map<DispatchTemplateLong>)
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<DispatchTemplateLong> insertCommand = repository.Insert<DispatchTemplateLong>();
                insertCommand.Insert
                    .ExcludeProperty(x => x.DispatchTemplateId);
                insertCommand.Output
                    .IncludeProperty(x => x.DispatchTemplateId);
                int changes = await insertCommand.ExecuteAsync(mappedList).ConfigureAwait(false);

                for (int i = 0; i < mappedList.Count; i++)
                {
                    items[i].DispatchTemplateId = mappedList[i].DispatchTemplateId;
                }
            }
        }


        //select
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<DispatchTemplate<long>>>> SelectPage(int pageIndex, int pageSize)
        {
            RepositoryResult<DispatchTemplateLong> response = null;
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                response = await repository.FindPageAsync<DispatchTemplateLong, long>(pageIndex, pageSize, true,
                        x => true, x => x.EventSettingsId, true)
                    .ConfigureAwait(false);                
            }

            List<DispatchTemplate<long>> mappedItems = response.Data
                .Select(_mapper.Map<DispatchTemplate<long>>)
                .ToList();
            return new TotalResult<List<DispatchTemplate<long>>>(mappedItems, response.TotalRows);
        }

        public virtual async Task<DispatchTemplate<long>> Select(long dispatchTemplatesId)
        {
            DispatchTemplateLong item = null;
           
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                item = await context.DispatchTemplates
                    .FirstOrDefaultAsync(x => x.DispatchTemplateId == dispatchTemplatesId)
                    .ConfigureAwait(false);
            }

            DispatchTemplate<long> mappedItem = item == null
                ? null
                : _mapper.Map<DispatchTemplate<long>>(item);
            return mappedItem;
        }

        public virtual async Task<List<DispatchTemplate<long>>> SelectForEventSettings(long eventSettingsId)
        {
            List<DispatchTemplateLong> items = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                items = await context.DispatchTemplates
                    .Where(x => x.EventSettingsId == eventSettingsId)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            List<DispatchTemplate<long>> mappedItems = items
                .Select(_mapper.Map<DispatchTemplate<long>>).ToList();
            return mappedItems;
        }


        //update
        public virtual async Task Update(List<DispatchTemplate<long>> items)
        {
            List<DispatchTemplateLong> mappedItems = items
                .Select(_mapper.Map<DispatchTemplateLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                MergeCommand<DispatchTemplateLong> merge = repository.Merge(mappedItems);
                merge.Compare
                    .IncludeProperty(p => p.DispatchTemplateId);
                merge.UpdateMatched
                    .ExcludeProperty(x => x.DispatchTemplateId);
                int changes = await merge.ExecuteAsync(MergeType.Update).ConfigureAwait(false);                
            }
        }


        //delete
        public virtual async Task Delete(List<DispatchTemplate<long>> items)
        {
            List<long> ids = items.Select(p => p.EventSettingsId)
                .Distinct()
                .ToList();

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<DispatchTemplateLong>(
                    x => ids.Contains(x.EventSettingsId))
                    .ConfigureAwait(false);                
            }
        }

    }
}
