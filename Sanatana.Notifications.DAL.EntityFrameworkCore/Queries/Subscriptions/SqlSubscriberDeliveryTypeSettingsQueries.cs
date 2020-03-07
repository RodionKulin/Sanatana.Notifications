using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SqlSubscriberDeliveryTypeSettingsQueries : ISubscriberDeliveryTypeSettingsQueries<long>
    {
        //fields        
        protected SqlConnectionSettings _connectionSettings;
        protected ISenderDbContextFactory _dbContextFactory;
        protected IMapper _mapper;


        //init
        public SqlSubscriberDeliveryTypeSettingsQueries(SqlConnectionSettings connectionSettings, 
            ISenderDbContextFactory dbContextFactory, INotificationsMapperFactory mapperFactory)
        {
            _connectionSettings = connectionSettings;
            _dbContextFactory = dbContextFactory;
            _mapper = mapperFactory.GetMapper();
        }


        //insert
        public virtual async Task Insert(List<SubscriberDeliveryTypeSettings<long>> items)
        {
            List<SubscriberDeliveryTypeSettingsLong> mappedList = items
                .Select(_mapper.Map<SubscriberDeliveryTypeSettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                InsertCommand<SubscriberDeliveryTypeSettingsLong> command = repository.Insert<SubscriberDeliveryTypeSettingsLong>();
                command.Insert.ExcludeProperty(x => x.SubscriberDeliveryTypeSettingsId);
                int changes = await command.ExecuteAsync(mappedList).ConfigureAwait(false);
            }
        }


        //select
        public virtual Task<bool> CheckAddressExists(int deliveryType, string address)
        {
            bool exist = false;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                exist = context.SubscriberDeliveryTypeSettings.Any(
                    x => x.Address == address
                    && x.DeliveryType == deliveryType);
            }

            return Task.FromResult(exist);
        }

        public virtual async Task<List<SubscriberDeliveryTypeSettings<long>>> Select(long subscriberId)
        {
            List<SubscriberDeliveryTypeSettingsLong> list = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                list = await context.SubscriberDeliveryTypeSettings
                    .Where(p => p.SubscriberId == subscriberId)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            
            return list.Cast<SubscriberDeliveryTypeSettings<long>>().ToList();
        }

        public virtual async Task<SubscriberDeliveryTypeSettings<long>> Select(long subscriberId, int deliveryType)
        {
            SubscriberDeliveryTypeSettings<long> item = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                item = await context.SubscriberDeliveryTypeSettings.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
            }

            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deliveryTypes"></param>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<TotalResult<List<SubscriberDeliveryTypeSettings<long>>>> SelectPage(
            List<int> deliveryTypes, int pageIndex, int pageSize)
        {
            RepositoryResult<SubscriberDeliveryTypeSettingsLong> result;

            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                result = await repository.FindPageAsync<SubscriberDeliveryTypeSettingsLong, long>(
                    pageIndex, pageSize, true
                    , x => deliveryTypes.Contains(x.DeliveryType)
                    , x => x.SubscriberId
                    , true)
                    .ConfigureAwait(false);
            }

            return result.ToTotalResult<SubscriberDeliveryTypeSettingsLong, SubscriberDeliveryTypeSettings<long>>();
        }

        public virtual async Task<List<SubscriberDeliveryTypeSettings<long>>> Select(
            int deliveryType, List<string> addresses)
        {
            List<SubscriberDeliveryTypeSettingsLong> list = null;
            
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                list = await context.SubscriberDeliveryTypeSettings.Where(
                    p => p.DeliveryType == deliveryType
                    && addresses.Contains(p.Address))
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            
            return list.Cast<SubscriberDeliveryTypeSettings<long>>().ToList();
        }



        //update
        public virtual async Task Update(SubscriberDeliveryTypeSettings<long> item)
        {
            SubscriberDeliveryTypeSettingsLong mappedItem = _mapper.Map<SubscriberDeliveryTypeSettingsLong>(item);
           
            using (SenderDbContext context = _dbContextFactory.GetDbContext())
            {
                context.SubscriberDeliveryTypeSettings.Attach(mappedItem);
                context.Entry(mappedItem).State = EntityState.Modified;
                int changes = await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateAddress(long subscriberId, int deliveryType, string address)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                UpdateCommand<SubscriberDeliveryTypeSettingsLong> command = repository.UpdateMany<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId
                    && x.DeliveryType == deliveryType);
                command.Assign(x => x.Address, x => address);
                int changes = await command.ExecuteAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateLastVisit(long subscriberId)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                UpdateCommand<SubscriberDeliveryTypeSettingsLong> updateCommand = repository.UpdateMany<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId);
                updateCommand.Assign(x => x.LastVisitUtc, x => DateTime.UtcNow);
                int changes = await updateCommand.ExecuteAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateTimeZone(long subscriberId, TimeZoneInfo timeZone)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                UpdateCommand<SubscriberDeliveryTypeSettingsLong> updateCommand = repository.UpdateMany<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId);
                updateCommand.Assign(x => x.TimeZoneId, x => timeZone.Id);
                int changes = await updateCommand.ExecuteAsync().ConfigureAwait(false);
            }            
        }

        public virtual async Task ResetNDRCount(long subscriberId, int deliveryType)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                UpdateCommand<SubscriberDeliveryTypeSettingsLong> updateCommand = repository.UpdateMany<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId
                    && x.DeliveryType == deliveryType);
                updateCommand.Assign(x => x.NDRCount, x => 0)
                    .Assign(x => x.IsNDRBlocked, x => false)
                    .Assign(x => x.NDRBlockResetCode, x => null)
                    .Assign(x => x.NDRBlockResetCodeSendDateUtc, x => null);
                int changes = await updateCommand.ExecuteAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateNDRResetCode(long subscriberId, int deliveryType, string resetCode)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                UpdateCommand<SubscriberDeliveryTypeSettingsLong> updateCommand = repository.UpdateMany<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId
                    && x.DeliveryType == deliveryType);
                updateCommand.Assign(x => x.NDRBlockResetCode, x => resetCode)
                    .Assign(x => x.NDRBlockResetCodeSendDateUtc, x => DateTime.UtcNow);
                int changes = await updateCommand.ExecuteAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task DisableAllDeliveryTypes(long subscriberId)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                UpdateCommand<SubscriberDeliveryTypeSettingsLong> updateCommand = repository.UpdateMany<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId);
                updateCommand.Assign(x => x.IsEnabled, x => false);
                int changes = await updateCommand.ExecuteAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task UpdateNDRSettings(List<SubscriberDeliveryTypeSettings<long>> items)
        {
            List<SubscriberDeliveryTypeSettingsLong> mappedList = items
                .Select(_mapper.Map<SubscriberDeliveryTypeSettingsLong>)
                .ToList();
            
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                string tvpName = TableValuedParameters.GetFullTVPName(_connectionSettings.Schema, TableValuedParameters.SUBSCRIBER_NDR_SETTINGS_TYPE);
                MergeCommand<SubscriberDeliveryTypeSettingsLong> merge = repository.MergeTVP(mappedList, tvpName);
                merge.Source
                    .IncludeProperty(p => p.SubscriberId)
                    .IncludeProperty(p => p.DeliveryType)
                    .IncludeProperty(p => p.NDRCount)
                    .IncludeProperty(p => p.IsNDRBlocked);
                merge.Compare
                    .IncludeProperty(p => p.SubscriberId)
                    .IncludeProperty(p => p.DeliveryType);
                merge.UpdateMatched
                    .IncludeProperty(p => p.NDRCount)
                    .IncludeProperty(p => p.IsNDRBlocked);
                int changes = await merge.ExecuteAsync(MergeType.Update).ConfigureAwait(false);
            }
        }


        //delete
        public virtual async Task Delete(long subscriberId, int deliveryType)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SubscriberDeliveryTypeSettingsLong>(
                    x => x.SubscriberId == subscriberId
                    && x.DeliveryType == deliveryType)
                    .ConfigureAwait(false);
            }
        }

        public virtual async Task Delete(long subscriberId)
        {
            using (Repository repository = new Repository(_dbContextFactory.GetDbContext()))
            {
                int changes = await repository.DeleteManyAsync<SubscriberScheduleSettingsLong>(
                    x => x.SubscriberId == subscriberId)
                    .ConfigureAwait(false);
            }
        }

    }
}
