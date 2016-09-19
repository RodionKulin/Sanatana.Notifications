using MongoDB.Bson;
using SignaloBot.DAL;
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
    public class MongoDbUserCategorySettingsQueries : IUserCategorySettingsQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbUserCategorySettingsQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }



        //методы
        public virtual async Task<bool> Insert(List<UserCategorySettings<ObjectId>> settings)
        {
            bool result = false;

            var options = new InsertManyOptions()
            {
                IsOrdered = false
            };

            try
            {
                await _context.UserCategorySettings.InsertManyAsync(settings, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;

        }

        public virtual async Task<QueryResult<List<UserCategorySettings<ObjectId>>>> Select(
            List<ObjectId> userIDs, int categoryID)
        {
            List<UserCategorySettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                    p => userIDs.Contains(p.UserID)
                    && p.CategoryID == categoryID);

                list = await _context.UserCategorySettings.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserCategorySettings<ObjectId>>();

            return new QueryResult<List<UserCategorySettings<ObjectId>>>(list, !result);
        }

        public virtual async Task<bool> UpsertIsEnabled(UserCategorySettings<ObjectId> settings)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                    p => p.UserID == settings.UserID
                    && p.CategoryID == settings.CategoryID
                    && p.DeliveryType == settings.DeliveryType);

                var update = Builders<UserCategorySettings<ObjectId>>.Update
                    .Set(p => p.IsEnabled, settings.IsEnabled);

                update = update.SetAllMappedMembers(settings);

                var options = new UpdateOptions()
                {
                    IsUpsert = true
                };

                UpdateResult response = await _context.UserCategorySettings.UpdateOneAsync(filter, update, options);
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
                var filter = Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                DeleteResult response = await _context.UserCategorySettings.DeleteManyAsync(filter);

                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        public virtual async Task<bool> Delete(ObjectId userID, int deliveryType, int categoryID)
        {
            bool result = false;

            try
            {
                var filter = Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.DeliveryType == deliveryType
                    && p.CategoryID == categoryID);

                DeleteResult response = await _context.UserCategorySettings.DeleteOneAsync(filter);

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
