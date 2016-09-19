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
using MongoDB.Driver.Core.Operations;

namespace SignaloBot.DAL.MongoDb
{
    public class MongoDbUserTopicSettingsQueries : IUserTopicSettingsQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbUserTopicSettingsQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }



        //методы
        public virtual async Task<bool> Insert(List<UserTopicSettings<ObjectId>> settings)
        {
            bool result = false;

            try
            {
                var options = new InsertManyOptions()
                {
                    IsOrdered = false
                };

                await _context.UserTopicSettings.InsertManyAsync(settings, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }



        public virtual async Task<QueryResult<UserTopicSettings<ObjectId>>> Select(
            ObjectId userID, int categoryID, string topicID)
        {
            UserTopicSettings<ObjectId> item = null;
            bool result = false;

            try
            {
                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.CategoryID == categoryID
                    && p.TopicID == topicID);

                //string explain = _context.UserTopicSettings.ExplainFind<UserTopicSettings<ObjectId>, UserTopicSettings<ObjectId>>
                //    (ExplainVerbosity.QueryPlanner, filter).Result.ToJsonIntended();

                item = await _context.UserTopicSettings.Find(filter).FirstOrDefaultAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }
            
            return new QueryResult<UserTopicSettings<ObjectId>>(item, !result);
        }

        public virtual async Task<TotalResult<List<UserTopicSettings<ObjectId>>>> SelectPage(
            ObjectId userID, List<int> categoryIDs, int page, int count)
        {
            List<UserTopicSettings<ObjectId>> list = null;
            long total = 0;
            bool result = false;

            try
            {
                int skip = MongoPaging.ToSkipNumber(page, count);

                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && categoryIDs.Contains(p.CategoryID)
                    && p.IsDeleted == false);

                Task<List<UserTopicSettings<ObjectId>>> listTask = _context.UserTopicSettings.Find(filter)
                    .Skip(skip)
                    .Limit(count)
                    .ToListAsync();
                
                Task<long> countTask = _context.UserTopicSettings.CountAsync(filter);

                list = await listTask;
                total = await countTask;
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserTopicSettings<ObjectId>>();

            return new TotalResult<List<UserTopicSettings<ObjectId>>>(list, total, !result);
        }


        
        public virtual async Task<bool> UpdateIsDeleted(UserTopicSettings<ObjectId> settings)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == settings.UserID
                    && p.CategoryID == settings.CategoryID
                    && p.TopicID == settings.TopicID);

                var update = Builders<UserTopicSettings<ObjectId>>.Update
                    .Set(p => p.IsDeleted, settings.IsDeleted);

                UpdateResult response = await _context.UserTopicSettings.UpdateOneAsync(filter, update);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }
        
        public virtual async Task<bool> Upsert(UserTopicSettings<ObjectId> settings, bool updateExisting)
        {
            bool result = true;

            try
            {
                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == settings.UserID
                    && p.CategoryID == settings.CategoryID
                    && p.TopicID == settings.TopicID);
                
                var update = Builders<UserTopicSettings<ObjectId>>.Update
                    .Combine()
                    .SetOnInsertAllMappedMembers(settings);

                if(updateExisting)
                {
                    update = update.Set(p => p.IsEnabled, settings.IsEnabled)
                        .Set(p => p.IsDeleted, settings.IsDeleted);
                }

                var options = new UpdateOptions()
                {
                    IsUpsert = true
                };

                UpdateResult response = await _context.UserTopicSettings.UpdateOneAsync(filter, update, options);
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
                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                DeleteResult response = await _context.UserTopicSettings.DeleteManyAsync(filter);
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        public virtual async Task<bool> Delete(ObjectId userID, int categoryID, string topicID)
        {
            bool result = false;

            try
            {
                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.CategoryID == categoryID
                    && p.TopicID == topicID);

                DeleteResult response = await _context.UserTopicSettings.DeleteManyAsync(filter);
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
