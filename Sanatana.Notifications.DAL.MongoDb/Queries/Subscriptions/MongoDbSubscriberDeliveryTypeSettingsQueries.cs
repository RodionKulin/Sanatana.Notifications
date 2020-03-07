using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.MongoDb;
using MongoDB.Driver;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberDeliveryTypeSettingsQueries : ISubscriberDeliveryTypeSettingsQueries<ObjectId>
    {
        //fields
        protected MongoDbConnectionSettings _settings;
        
        protected SenderMongoDbContext _context;


        //init
        public MongoDbSubscriberDeliveryTypeSettingsQueries(MongoDbConnectionSettings connectionSettings)
        {
            
            _settings = connectionSettings;
            _context = new SenderMongoDbContext(connectionSettings);
        }



        //methods
        public virtual async Task Insert(List<SubscriberDeliveryTypeSettings<ObjectId>> settings)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _context.SubscriberDeliveryTypeSettings.InsertManyAsync(settings, options);
        }



        public virtual async Task<bool> CheckAddressExists(
            int deliveryType, string address)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.Address == address
                    && p.DeliveryType == deliveryType);

            long count = await _context.SubscriberDeliveryTypeSettings.CountDocumentsAsync(filter);
            bool exists = count > 0;

            return exists;
        }

        public virtual async Task<List<SubscriberDeliveryTypeSettings<ObjectId>>> Select(
            ObjectId subscriberId)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            List<SubscriberDeliveryTypeSettings<ObjectId>> list = await _context.SubscriberDeliveryTypeSettings.Find(filter)
                .ToListAsync();

            return list;
        }

        public virtual async Task<SubscriberDeliveryTypeSettings<ObjectId>> Select(
            ObjectId subscriberId, int deliveryType)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            SubscriberDeliveryTypeSettings<ObjectId> item = await _context.SubscriberDeliveryTypeSettings.Find(filter).FirstOrDefaultAsync();

            return item;
        }

        public virtual async Task<List<SubscriberDeliveryTypeSettings<ObjectId>>> Select(
            int deliveryType, List<string> addresses)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => addresses.Contains(p.Address)
                    && p.DeliveryType == deliveryType);

            List<SubscriberDeliveryTypeSettings<ObjectId>> list = await _context.SubscriberDeliveryTypeSettings.Find(filter)
                .ToListAsync();

            return list;
        }

        public virtual async Task<TotalResult<List<SubscriberDeliveryTypeSettings<ObjectId>>>> SelectPage(
            List<int> deliveryTypes, int pageIndex, int pageSize)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);

            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => deliveryTypes.Contains(p.DeliveryType));

            Task<List<SubscriberDeliveryTypeSettings<ObjectId>>> listTask = _context.SubscriberDeliveryTypeSettings
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            Task<long> totalTask = _context.SubscriberDeliveryTypeSettings.CountDocumentsAsync(filter);

            List<SubscriberDeliveryTypeSettings<ObjectId>> list = await listTask;
            long total = await totalTask;

            return new TotalResult<List<SubscriberDeliveryTypeSettings<ObjectId>>>(list, total);
        }



        public virtual async Task DisableAllDeliveryTypes(ObjectId subscriberId)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Set(p => p.IsEnabled, false);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateManyAsync(filter, update);
        }

        public virtual async Task ResetNDRCount(ObjectId subscriberId, int deliveryType)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Set(p => p.NDRCount, 0);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateOneAsync(filter, update);
        }

        public virtual async Task Update(SubscriberDeliveryTypeSettings<ObjectId> settings)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == settings.SubscriberId
                    && p.DeliveryType == settings.DeliveryType);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Combine()
                .SetAllMappedMembers(settings);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateOneAsync(filter, update);
        }

        public virtual async Task UpdateAddress(ObjectId subscriberId, int deliveryType, string address)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Set(p => p.Address, address);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateOneAsync(filter, update);
        }

        public virtual async Task UpdateLastVisit(ObjectId subscriberId)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Set(p => p.LastVisitUtc, DateTime.UtcNow);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateManyAsync(filter, update);
        }

        public virtual async Task UpdateNDRResetCode(ObjectId subscriberId, int deliveryType, string resetCode)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Set(p => p.NDRBlockResetCode, resetCode);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateOneAsync(filter, update);
        }

        public virtual async Task UpdateNDRSettings(List<SubscriberDeliveryTypeSettings<ObjectId>> settings)
        {
            var updates = new List<WriteModel<SubscriberDeliveryTypeSettings<ObjectId>>>();

            foreach (SubscriberDeliveryTypeSettings<ObjectId> item in settings)
            {
                var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == item.SubscriberId
                    && p.DeliveryType == item.DeliveryType);

                var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.NDRCount, item.NDRCount)
                    .Set(p => p.IsNDRBlocked, item.IsNDRBlocked);

                updates.Add(new UpdateOneModel<SubscriberDeliveryTypeSettings<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _context.SubscriberDeliveryTypeSettings.BulkWriteAsync(updates, options);
        }

        public virtual async Task UpdateTimeZone(ObjectId subscriberId, TimeZoneInfo timeZone)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            var update = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Update
                .Set(p => p.TimeZoneId, timeZone.DisplayName);

            UpdateResult response = await _context.SubscriberDeliveryTypeSettings.UpdateManyAsync(filter, update);
        }



        public virtual async Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            DeleteResult response = await _context.SubscriberDeliveryTypeSettings.DeleteManyAsync(filter);

        }

        public virtual async Task Delete(ObjectId subscriberId, int deliveryType)
        {
            var filter = Builders<SubscriberDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            DeleteResult response = await _context.SubscriberDeliveryTypeSettings.DeleteOneAsync(filter);

        }

    }
}
