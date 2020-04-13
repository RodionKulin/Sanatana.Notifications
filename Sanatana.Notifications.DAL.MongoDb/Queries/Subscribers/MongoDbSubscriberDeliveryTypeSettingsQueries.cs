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
using Sanatana.Notifications.DAL.MongoDb.Context;
using Sanatana.Notifications.DAL.MongoDb.Entities;

namespace Sanatana.Notifications.DAL.MongoDb.Queries
{
    public class MongoDbSubscriberDeliveryTypeSettingsQueries<TDeliveryType, TCategory> : ISubscriberDeliveryTypeSettingsQueries<TDeliveryType, ObjectId>
        where TDeliveryType : MongoDbSubscriberDeliveryTypeSettings<TCategory>
        where TCategory : SubscriberCategorySettings<ObjectId>
    {
        //fields
        protected ICollectionFactory _collectionFactory;


        //init
        public MongoDbSubscriberDeliveryTypeSettingsQueries(ICollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory;
        }



        //methods
        public virtual async Task Insert(List<TDeliveryType> settings)
        {
            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            await _collectionFactory
                .GetCollection<TDeliveryType>()
                .InsertManyAsync(settings, options)
                .ConfigureAwait(false);
        }



        public virtual async Task<bool> CheckAddressExists(int deliveryType, string address)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.Address == address
                    && p.DeliveryType == deliveryType);

            long count = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .CountDocumentsAsync(filter)
                .ConfigureAwait(false);
            bool exists = count > 0;

            return exists;
        }

        public virtual async Task<List<TDeliveryType>> Select(
            ObjectId subscriberId)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                p => p.SubscriberId == subscriberId);

            List<TDeliveryType> list = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .Find(filter)
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        public virtual async Task<TDeliveryType> Select(
            ObjectId subscriberId, int deliveryType)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            TDeliveryType item = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .Find(filter)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return item;
        }

        public virtual async Task<List<TDeliveryType>> Select(
            int deliveryType, List<string> addresses)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                p => addresses.Contains(p.Address)
                && p.DeliveryType == deliveryType);

            List<TDeliveryType> list = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .Find(filter)
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        public virtual async Task<TotalResult<List<TDeliveryType>>> SelectPage(
            List<int> deliveryTypes, int pageIndex, int pageSize)
        {
            int skip = MongoDbPageNumbers.ToSkipNumber(pageIndex, pageSize);

            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => deliveryTypes.Contains(p.DeliveryType));

            Task<List<TDeliveryType>> listTask = _collectionFactory
                .GetCollection<TDeliveryType>()
                .Find(filter)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            Task<long> totalTask = _collectionFactory
                .GetCollection<TDeliveryType>()
                .CountDocumentsAsync(filter);

            List<TDeliveryType> list = await listTask.ConfigureAwait(false);
            long total = await totalTask.ConfigureAwait(false);

            return new TotalResult<List<TDeliveryType>>(list, total);
        }



        public virtual async Task DisableAllDeliveryTypes(ObjectId subscriberId)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                p => p.SubscriberId == subscriberId);

            var update = Builders<TDeliveryType>.Update
                .Set(p => p.IsEnabled, false);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateManyAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task ResetNDRCount(ObjectId subscriberId, int deliveryType)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            var update = Builders<TDeliveryType>.Update
                .Set(p => p.NDRCount, 0);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task Update(TDeliveryType settings)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == settings.SubscriberId
                    && p.DeliveryType == settings.DeliveryType);

            var update = Builders<TDeliveryType>.Update
                .Combine()
                .SetAllMappedMembers(settings);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateAddress(ObjectId subscriberId, int deliveryType, string address)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            var update = Builders<TDeliveryType>.Update
                .Set(p => p.Address, address);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateLastVisit(ObjectId subscriberId)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            var update = Builders<TDeliveryType>.Update
                .Set(p => p.LastVisitUtc, DateTime.UtcNow);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateManyAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateNDRResetCode(ObjectId subscriberId, int deliveryType, string resetCode)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            var update = Builders<TDeliveryType>.Update
                .Set(p => p.NDRBlockResetCode, resetCode);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateNDRSettings(List<TDeliveryType> settings)
        {
            var updates = new List<WriteModel<TDeliveryType>>();

            foreach (TDeliveryType item in settings)
            {
                var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == item.SubscriberId
                    && p.DeliveryType == item.DeliveryType);

                var update = Builders<TDeliveryType>.Update
                    .Set(p => p.NDRCount, item.NDRCount)
                    .Set(p => p.IsNDRBlocked, item.IsNDRBlocked);

                updates.Add(new UpdateOneModel<TDeliveryType>(filter, update)
                {
                    IsUpsert = false
                });
            }

            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .BulkWriteAsync(updates, options)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateTimeZone(ObjectId subscriberId, TimeZoneInfo timeZone)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            var update = Builders<TDeliveryType>.Update
                .Set(p => p.TimeZoneId, timeZone.DisplayName);

            UpdateResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .UpdateManyAsync(filter, update)
                .ConfigureAwait(false);
        }



        public virtual async Task Delete(ObjectId subscriberId)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId);

            DeleteResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);
        }

        public virtual async Task Delete(ObjectId subscriberId, int deliveryType)
        {
            var filter = Builders<TDeliveryType>.Filter.Where(
                    p => p.SubscriberId == subscriberId
                    && p.DeliveryType == deliveryType);

            DeleteResult response = await _collectionFactory
                .GetCollection<TDeliveryType>()
                .DeleteOneAsync(filter)
                .ConfigureAwait(false);
        }

    }
}
