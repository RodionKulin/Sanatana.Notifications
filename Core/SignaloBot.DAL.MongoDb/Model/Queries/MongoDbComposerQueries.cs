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
    public class MongoDbComposerQueries : IComposerQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbComposerQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }



        //методы
        public virtual async Task<bool> Insert(List<ComposerSettings<ObjectId>> items)
        {
            bool result = false;

            try
            {
                foreach (ComposerSettings<ObjectId> item in items)
                {
                    item.ComposerSettingsID = ObjectId.GenerateNewId();
                }

                var options = new InsertManyOptions()
                {
                    IsOrdered = false
                };

                await _context.ComposerSettings.InsertManyAsync(items, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<QueryResult<ComposerSettings<ObjectId>>> Select(
            ObjectId composerSettingsID)
        {
            ComposerSettings<ObjectId> item = null;
            bool result = false;

            try
            {
                var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                    p => p.ComposerSettingsID == composerSettingsID);

                item = await _context.ComposerSettings.Find(filter).FirstOrDefaultAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }
            
            return new QueryResult<ComposerSettings<ObjectId>>(item, !result);
        }

        public virtual async Task<QueryResult<List<ComposerSettings<ObjectId>>>> Select(
            int category)
        {
            List<ComposerSettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                    p => p.CategoryID == category);
                
                list = await _context.ComposerSettings.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<ComposerSettings<ObjectId>>();

            return new QueryResult<List<ComposerSettings<ObjectId>>>(list, !result);
        }

        public virtual async Task<bool> Update(List<ComposerSettings<ObjectId>> items)
        {
            bool result = true;

            try
            {
                var requests = new List<WriteModel<ComposerSettings<ObjectId>>>();

                foreach (ComposerSettings<ObjectId> item in items)
                {
                    var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                        p => p.ComposerSettingsID == item.ComposerSettingsID);

                    var update = Builders<ComposerSettings<ObjectId>>.Update
                        .Set(p => p.Subscribtion, item.Subscribtion)
                        .Set(p => p.Updates, item.Updates)
                        .Set(p => p.Templates, item.Templates);

                    requests.Add(new UpdateOneModel<ComposerSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }

                var options = new BulkWriteOptions
                {
                    IsOrdered = false
                };

                BulkWriteResult response = await _context.ComposerSettings
                    .BulkWriteAsync(requests, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> Delete(List<ComposerSettings<ObjectId>> items)
        {
            bool result = false;

            try
            {
                List<ObjectId> ids = items.Select(p => p.ComposerSettingsID).ToList();

                var filter = Builders<ComposerSettings<ObjectId>>.Filter.Where(
                    p => ids.Contains(p.ComposerSettingsID));

                DeleteResult response = await _context.ComposerSettings.DeleteManyAsync(filter);
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
