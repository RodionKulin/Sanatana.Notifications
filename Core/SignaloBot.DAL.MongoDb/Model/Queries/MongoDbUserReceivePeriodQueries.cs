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
    public class MongoDbUserReceivePeriodQueries : IUserReceivePeriodQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbUserReceivePeriodQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }



        //методы
        public virtual async Task<bool> Insert(List<UserReceivePeriod<ObjectId>> periods)
        {
            bool result = false;

            try
            {
                var options = new InsertManyOptions()
                {
                    IsOrdered = false
                };

                await _context.UserReceivePeriods.InsertManyAsync(periods, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }



        public virtual async Task<QueryResult<List<UserReceivePeriod<ObjectId>>>> Select(
            ObjectId userID)
        {
            List<UserReceivePeriod<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserReceivePeriod<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                list = await _context.UserReceivePeriods.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserReceivePeriod<ObjectId>>();

            return new QueryResult<List<UserReceivePeriod<ObjectId>>>(list, !result);
        }
        
        public virtual async Task<QueryResult<List<UserReceivePeriod<ObjectId>>>> Select(
            List<ObjectId> userIDs, List<int> receivePeriodsGroups)
        {
            List<UserReceivePeriod<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserReceivePeriod<ObjectId>>.Filter.Where(
                    p => userIDs.Contains(p.UserID)
                    && receivePeriodsGroups.Contains(p.ReceivePeriodsGroupID));
                
                list = await _context.UserReceivePeriods.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserReceivePeriod<ObjectId>>();

            return new QueryResult<List<UserReceivePeriod<ObjectId>>>(list, !result);
        }



        public virtual async Task<bool> Rewrite(ObjectId userID, int receivePeriodsGroup
            , List<UserReceivePeriod<ObjectId>> periods)
        {
            bool result = true;

            try
            {
                var requests = new List<WriteModel<UserReceivePeriod<ObjectId>>>();

                var filter = Builders<UserReceivePeriod<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && p.ReceivePeriodsGroupID == receivePeriodsGroup);                
                requests.Add(new DeleteManyModel<UserReceivePeriod<ObjectId>>(filter));

                foreach (UserReceivePeriod<ObjectId> item in periods)
                {
                    requests.Add(new InsertOneModel<UserReceivePeriod<ObjectId>>(item));
                }

                var options = new BulkWriteOptions()
                {
                    IsOrdered = true
                };

                BulkWriteResult response = await _context.UserReceivePeriods.BulkWriteAsync(requests, options);
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
                var filter = Builders<UserReceivePeriod<ObjectId>>.Filter.Where(
                    p => p.UserID == userID);

                DeleteResult response = await _context.UserReceivePeriods.DeleteManyAsync(filter);
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }
        
        public virtual async Task<bool> Delete(ObjectId userID, List<int> receivePeriodsGroups)
        {
            bool result = false;

            try
            {
                var filter = Builders<UserReceivePeriod<ObjectId>>.Filter.Where(
                    p => p.UserID == userID
                    && receivePeriodsGroups.Contains(p.ReceivePeriodsGroupID));

                DeleteResult response = await _context.UserReceivePeriods.DeleteManyAsync(filter);
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
