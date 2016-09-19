using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.MongoDb
{
    public class MongoDbSignalDispatchQueries 
        : ISignalDispatchQueries<ObjectId>, ISignalDispatchQueueQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbSignalDispatchQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }

        

        //Insert
        public virtual async Task<bool> Insert(List<SignalDispatchBase<ObjectId>> items)
        {
            bool result = false;

            try
            {
                foreach (SignalDispatchBase<ObjectId> item in items)
                {
                    item.SignalDispatchID = ObjectId.GenerateNewId();
                }

                var options = new InsertManyOptions()
                {
                    IsOrdered = false
                };
                
                await _context.SignalDispatches.InsertManyAsync(items, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }


        //Select
        public virtual async Task<QueryResult<List<SignalDispatchBase<ObjectId>>>> Select(
            int count, List<int> deliveryTypes, int maxFailedAttempts)
        {
            List<SignalDispatchBase<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<SignalDispatchBase<ObjectId>>.Filter.Where(
                    p => p.SendDateUtc <= DateTime.UtcNow
                    && deliveryTypes.Contains(p.DeliveryType)
                    && p.FailedAttempts < maxFailedAttempts);

                var options = new FindOptions()
                {
                    AllowPartialResults = true
                };

                list = await _context.SignalDispatches.Find(filter, options)
                    .SortBy(p => p.SendDateUtc)
                    .Limit(count)
                    .ToListAsync();

                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<SignalDispatchBase<ObjectId>>();

            return new QueryResult<List<SignalDispatchBase<ObjectId>>>(list, !result);
        }
        
        public virtual async Task<QueryResult<List<SignalDispatchBase<ObjectId>>>> SelectDelayed(
            ObjectId userID, List<KeyValuePair<int, int>> deliveryTypeAndCategories)
        {
            bool result = false;
            List<SignalDispatchBase<ObjectId>> list = null;
                
            try
            {
                var categoryFilter = Builders<SignalDispatchBase<ObjectId>>.Filter.Where(p => false);
                foreach (KeyValuePair<int, int> category in deliveryTypeAndCategories)
                {
                    categoryFilter = Builders<SignalDispatchBase<ObjectId>>.Filter.Or(categoryFilter, Builders<SignalDispatchBase<ObjectId>>.Filter.Where(
                        p => p.DeliveryType == category.Key
                        && p.CategoryID == category.Value));
                }

                var filter = Builders<SignalDispatchBase<ObjectId>>.Filter.Where(
                    p => p.ReceiverUserID == userID
                    && p.IsDelayed == true);
                filter = Builders<SignalDispatchBase<ObjectId>>.Filter.And(filter, categoryFilter);

                list = await _context.SignalDispatches.Find(filter).ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)            
                list = new List<SignalDispatchBase<ObjectId>>();
            
            return new QueryResult<List<SignalDispatchBase<ObjectId>>>(list, !result);
        }

        
        //Update
        public virtual async Task<bool> UpdateSendDateUtc(List<SignalDispatchBase<ObjectId>> items)
        {
            bool result = false;
                      
            try
            {
                var operations = new List<WriteModel<SignalDispatchBase<ObjectId>>>();
                foreach (SignalDispatchBase<ObjectId> item in items)
                {
                    var filter = Builders<SignalDispatchBase<ObjectId>>.Filter.Where(
                        p => p.SignalDispatchID == item.SignalDispatchID);

                    var update = Builders<SignalDispatchBase<ObjectId>>.Update
                        .Set(p => p.SendDateUtc, item.SendDateUtc)
                        .Set(p => p.IsDelayed, item.IsDelayed);

                    operations.Add(new UpdateOneModel<SignalDispatchBase<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }

                var options = new BulkWriteOptions()
                {
                    IsOrdered = false
                };
                
                BulkWriteResult response = 
                    await _context.SignalDispatches.BulkWriteAsync(operations, options);
                
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }

        public virtual async Task<bool> UpdateCounters(
            UpdateParameters parameters, List<SignalDispatchBase<ObjectId>> items)
        {
            if (!parameters.UpdateAnything)
            {
                return true;
            }

            bool result = false;

            try
            {
                Task<bool> dtTask = UpdateDTCounters(parameters, items);
                Task<bool> categoryTask = UpdateCategoryCounters(parameters, items);
                Task<bool> topicTask = UpdateTopicCounters(parameters, items);

                bool dtResult = await dtTask;
                bool categoryResult = await categoryTask;
                bool topicResult = await topicTask;

                result = dtResult && categoryResult && topicResult;

            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }
        protected virtual async Task<bool> UpdateDTCounters(
            UpdateParameters parameters, List<SignalDispatchBase<ObjectId>> items)
        {
            if (!parameters.UpdateDeliveryType)
            {
                return true;
            }
            
            bool result = false;

            try
            {
                var operations = new List<WriteModel<UserDeliveryTypeSettings<ObjectId>>>();
                foreach (SignalDispatchBase<ObjectId> item in items)
                {
                    var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                        p => p.UserID == item.ReceiverUserID.Value
                        && p.DeliveryType == item.DeliveryType);

                    var update = Builders<UserDeliveryTypeSettings<ObjectId>>.Update.Combine();
                    if(parameters.UpdateDeliveryTypeLastSendDateUtc)
                    {
                        update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                    }
                    if (parameters.UpdateDeliveryTypeSendCount)
                    {
                        update = update.Inc(p => p.SendCount, 1);
                    }
                    
                    operations.Add(new UpdateOneModel<UserDeliveryTypeSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }

                var options = new BulkWriteOptions()
                {
                    IsOrdered = false
                };

                BulkWriteResult<UserDeliveryTypeSettings<ObjectId>> response =
                    await _context.UserDeliveryTypeSettings.BulkWriteAsync(operations, options);

                result = true;
                
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }
        protected virtual async Task<bool> UpdateCategoryCounters(
            UpdateParameters parameters, List<SignalDispatchBase<ObjectId>> items)
        {
            if (!parameters.UpdateCategory)
            {
                return true;
            }

            bool result = false;

            try
            {
                var operations = new List<WriteModel<UserCategorySettings<ObjectId>>>();
                foreach (SignalDispatchBase<ObjectId> item in items)
                {
                    var filter = Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                        p => p.UserID == item.ReceiverUserID.Value
                        && p.DeliveryType == item.DeliveryType);

                    var update = Builders<UserCategorySettings<ObjectId>>.Update.Combine();
                    if (parameters.UpdateDeliveryTypeLastSendDateUtc)
                    {
                        update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                    }
                    if (parameters.UpdateDeliveryTypeSendCount)
                    {
                        update = update.Inc(p => p.SendCount, 1);
                    }

                    if(parameters.CreateCategoryIfNotExist)
                    {
                        update = update.SetOnInsertAllMappedMembers(new UserCategorySettings<ObjectId>()
                        {
                            UserID = item.ReceiverUserID.Value,
                            DeliveryType = item.DeliveryType,
                            CategoryID = item.CategoryID,
                            LastSendDateUtc = item.SendDateUtc,
                            SendCount = 1,
                            IsEnabled = true,
                        });
                    }

                    operations.Add(new UpdateOneModel<UserCategorySettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = parameters.CreateCategoryIfNotExist
                    });
                }

                var options = new BulkWriteOptions()
                {
                    IsOrdered = false
                };

                BulkWriteResult<UserCategorySettings<ObjectId>> response =
                    await _context.UserCategorySettings.BulkWriteAsync(operations, options);

                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }
        protected virtual async Task<bool> UpdateTopicCounters(
            UpdateParameters parameters, List<SignalDispatchBase<ObjectId>> items)
        {
            if (!parameters.UpdateTopic)
            {
                return true;
            }

            bool result = false;

            try
            {
                var operations = new List<WriteModel<UserTopicSettings<ObjectId>>>();
                foreach (SignalDispatchBase<ObjectId> item in items)
                {
                    var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                        p => p.UserID == item.ReceiverUserID.Value
                        && p.CategoryID == item.CategoryID
                        && p.TopicID == item.TopicID);

                    var update = Builders<UserTopicSettings<ObjectId>>.Update.Combine();
                    if (parameters.UpdateDeliveryTypeLastSendDateUtc)
                    {
                        update = update.Set(p => p.LastSendDateUtc, item.SendDateUtc);
                    }
                    if (parameters.UpdateDeliveryTypeSendCount)
                    {
                        update = update.Inc(p => p.SendCount, 1);
                    }

                    if (parameters.CreateCategoryIfNotExist)
                    {
                        update = update.SetOnInsertAllMappedMembers(new UserTopicSettings<ObjectId>()
                        {
                            UserID = item.ReceiverUserID.Value,
                            CategoryID = item.CategoryID,
                            TopicID = item.TopicID,
                            LastSendDateUtc = item.SendDateUtc,
                            SendCount = 1,
                            AddDateUtc = DateTime.UtcNow,
                            IsEnabled = true,
                            IsDeleted = false
                        });
                    }

                    operations.Add(new UpdateOneModel<UserTopicSettings<ObjectId>>(filter, update)
                    {
                        IsUpsert = parameters.CreateTopicIfNotExist
                    });
                }

                var options = new BulkWriteOptions()
                {
                    IsOrdered = false
                };

                BulkWriteResult<UserTopicSettings<ObjectId>> response =
                    await _context.UserTopicSettings.BulkWriteAsync(operations, options);

                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }
                
        public async Task<bool> UpdateFailedAttempts(List<SignalDispatchBase<ObjectId>> items)
        {
            bool result = true;

            try
            {
                var requests = new List<WriteModel<SignalDispatchBase<ObjectId>>>();

                foreach (SignalDispatchBase<ObjectId> item in items)
                {
                    var filter = Builders<SignalDispatchBase<ObjectId>>.Filter.Where(
                        p => p.SignalDispatchID == item.SignalDispatchID);

                    var update = Builders<SignalDispatchBase<ObjectId>>.Update
                        .Set(p => p.FailedAttempts, item.FailedAttempts)
                        .Set(p => p.SendDateUtc, item.SendDateUtc)
                        .Set(p => p.IsDelayed, item.IsDelayed);

                    requests.Add(new UpdateOneModel<SignalDispatchBase<ObjectId>>(filter, update)
                    {
                        IsUpsert = false
                    });
                }

                var options = new BulkWriteOptions
                {
                    IsOrdered = false
                };

                BulkWriteResult response = await _context.SignalDispatches
                    .BulkWriteAsync(requests, options);
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
            }

            return result;
        }
        


        //Delete
        public virtual async Task<bool> Delete(List<SignalDispatchBase<ObjectId>> items)
        {
            bool result = false;

            try
            {
                List<ObjectId> ids = items
                    .Select(p => p.SignalDispatchID).ToList();

                var filter = Builders<SignalDispatchBase<ObjectId>>.Filter.Where(
                    p => ids.Contains(p.SignalDispatchID));

                DeleteResult response = await _context.SignalDispatches.DeleteManyAsync(filter);
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        
        //IDisposable
        public virtual void Dispose()
        {

        }

        


    }
}
