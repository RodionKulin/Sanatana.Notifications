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
    public class MongoDbSignalEventQueries : ISignalEventQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbSignalEventQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }



        //методы
        public virtual async Task<bool> Insert(List<SignalEventBase<ObjectId>> items)
        {
            bool result = false;

            try
            {
                foreach (SignalEventBase<ObjectId> item in items)
                {
                    item.SignalEventID = ObjectId.GenerateNewId();
                }

                var options = new InsertManyOptions()
                {
                    IsOrdered = false
                };

                await _context.SignalEvents.InsertManyAsync(items, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<QueryResult<List<SignalEventBase<ObjectId>>>> Select(
            int count, int maxFailedAttempts)
        {
            List<SignalEventBase<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<SignalEventBase<ObjectId>>.Filter.Where(
                    p => p.FailedAttempts < maxFailedAttempts);

                var options = new FindOptions()
                {
                    AllowPartialResults = true
                };

                list = await _context.SignalEvents.Find(filter, options)
                    .SortBy(p => p.ReceiveDateUtc)
                    .Limit(count)
                    .ToListAsync();

                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<SignalEventBase<ObjectId>>();

            return new QueryResult<List<SignalEventBase<ObjectId>>>(list, !result);
        }

        public virtual async Task<bool> Update(List<SignalEventBase<ObjectId>> items)
        {
            bool result = true;

            try
            {
                var requests = new List<WriteModel<SignalEventBase<ObjectId>>>();

                foreach (SignalEventBase<ObjectId> item in items)
                {
                    var filter = Builders<SignalEventBase<ObjectId>>.Filter.Where(
                        p => p.SignalEventID == item.SignalEventID);

                    var update = Builders<SignalEventBase<ObjectId>>.Update
                        .Set(p => p.FailedAttempts, item.FailedAttempts)
                        .Set(p => p.ComposerSettingsID, item.ComposerSettingsID)
                        .Set(p => p.IsSplitted, item.IsSplitted)
                        .Set(p => p.UserIDRangeFrom, item.UserIDRangeFrom)
                        .Set(p => p.UserIDRangeTo, item.UserIDRangeTo);

                    requests.Add(new UpdateOneModel<SignalEventBase<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }

                var options = new BulkWriteOptions
                {
                    IsOrdered = false
                };

                BulkWriteResult response = await _context.SignalEvents
                    .BulkWriteAsync(requests, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> Delete(List<SignalEventBase<ObjectId>> items)
        {
            bool result = false;

            try
            {
                List<ObjectId> ids = items.Select(p => p.SignalEventID).ToList();

                var filter = Builders<SignalEventBase<ObjectId>>.Filter.Where(
                    p => ids.Contains(p.SignalEventID));

                DeleteResult response = await _context.SignalEvents.DeleteManyAsync(filter);
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }



        public virtual void Dispose()
        {
        }
    }
}
