using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using Common.MongoDb;
using MongoDB.Driver;

namespace SignaloBot.DAL.MongoDb
{
    public class MongoDbUserDeliveryTypeSettingsQueries : IUserDeliveryTypeSettingsQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbUserDeliveryTypeSettingsQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }



        //методы
        public virtual async Task<bool> Insert(List<UserDeliveryTypeSettings<ObjectId>> settings)
        {
            bool result = false;

            try
            {
                var options = new InsertManyOptions()
                {
                    IsOrdered = false
                };

                await _context.UserDeliveryTypeSettings.InsertManyAsync(settings, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }



        public virtual async Task<QueryResult<bool>> CheckAddressExists(
            int deliveryType, string address)
        {
            bool exists = false;
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.Address == address
                    && p.DeliveryType == deliveryType);

                long count = await _context.UserDeliveryTypeSettings.CountAsync(filter);

                exists = count > 0;
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return new QueryResult<bool>(exists, !result);
        }

        public virtual async Task<QueryResult<List<UserDeliveryTypeSettings<ObjectId>>>> Select(
            ObjectId userID)
        {
            List<UserDeliveryTypeSettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                list = await _context.UserDeliveryTypeSettings.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettings<ObjectId>>();

            return new QueryResult<List<UserDeliveryTypeSettings<ObjectId>>>(list, !result);
        }

        public virtual async Task<QueryResult<UserDeliveryTypeSettings<ObjectId>>> Select(
            ObjectId userID, int deliveryType)
        {
            UserDeliveryTypeSettings<ObjectId> item = null;
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.DeliveryType == deliveryType);

                item = await _context.UserDeliveryTypeSettings.Find(filter).FirstOrDefaultAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return new QueryResult<UserDeliveryTypeSettings<ObjectId>>(item, !result);
        }

        public virtual async Task<QueryResult<List<UserDeliveryTypeSettings<ObjectId>>>> Select(
            int deliveryType, List<string> addresses)
        {
            List<UserDeliveryTypeSettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => addresses.Contains(p.Address)
                    && p.DeliveryType == deliveryType);

                list = await _context.UserDeliveryTypeSettings.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettings<ObjectId>>();

            return new QueryResult<List<UserDeliveryTypeSettings<ObjectId>>>(list, !result);
        }

        public virtual async Task<TotalResult<List<UserDeliveryTypeSettings<ObjectId>>>> SelectPage(
            List<int> deliveryTypes, int page, int pageSize)
        {
            List<UserDeliveryTypeSettings<ObjectId>> list = null;
            long total = 0;
            bool result = false;

            int skip = MongoPaging.ToSkipNumber(page, pageSize);
            
            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => deliveryTypes.Contains(p.DeliveryType));

                Task<List<UserDeliveryTypeSettings<ObjectId>>> listTask = _context.UserDeliveryTypeSettings
                    .Find(filter)
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();

                Task<long> totalTask = _context.UserDeliveryTypeSettings.CountAsync(filter);

                list = await listTask;
                total = await totalTask;
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettings<ObjectId>>();

            return new TotalResult<List<UserDeliveryTypeSettings<ObjectId>>>(list, total, !result);
        }



        public virtual async Task<bool> DisableAllDeliveryTypes(ObjectId userID)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.IsEnabled, false);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateManyAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> ResetNDRCount(ObjectId userID, int deliveryType)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.DeliveryType == deliveryType);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.NDRCount, 0);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateOneAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> Update(UserDeliveryTypeSettings<ObjectId> settings)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == settings.UserID
                    && p.DeliveryType == settings.DeliveryType);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Combine()
                    .SetAllMappedMembers(settings);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateOneAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> UpdateAddress(ObjectId userID, int deliveryType, string address)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.DeliveryType == deliveryType);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.Address, address);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateOneAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> UpdateLastVisit(ObjectId userID)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.LastUserVisitUtc, DateTime.UtcNow);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateManyAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> UpdateNDRResetCode(ObjectId userID, int deliveryType, string resetCode)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.DeliveryType == deliveryType);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.BlockOfNDRResetCode, resetCode);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateOneAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> UpdateNDRSettings(List<UserDeliveryTypeSettings<ObjectId>> settings)
        {
            bool result = true;

            try
            {
                var updates = new List<WriteModel<UserDeliveryTypeSettings<ObjectId>>>();

                foreach (UserDeliveryTypeSettings<ObjectId> item in settings)
                {
                    var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.UserID == item.UserID
                        && p.DeliveryType == item.DeliveryType);

                    var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                        .Set(p => p.NDRCount, item.NDRCount)
                        .Set(p => p.IsBlockedOfNDR, item.IsBlockedOfNDR);

                    updates.Add(new UpdateOneModel<UserDeliveryTypeSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }

                var options = new BulkWriteOptions()
                {
                    IsOrdered = false
                };

                BulkWriteResult response = await _context.UserDeliveryTypeSettings.BulkWriteAsync(updates, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> UpdateTimeZone(ObjectId userID, TimeZoneInfo timeZone)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update
                    .Set(p => p.TimeZoneID, timeZone.DisplayName);

                UpdateResult response = await _context.UserDeliveryTypeSettings.UpdateManyAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }



        public virtual async Task<bool> Delete(ObjectId userID)
        {
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                DeleteResult response = await _context.UserDeliveryTypeSettings.DeleteManyAsync(filter);
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;

        }

        public virtual async Task<bool> Delete(ObjectId userID, int deliveryType)
        {
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.DeliveryType == deliveryType);

                DeleteResult response = await _context.UserDeliveryTypeSettings.DeleteOneAsync(filter);
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;

        }

    }
}
