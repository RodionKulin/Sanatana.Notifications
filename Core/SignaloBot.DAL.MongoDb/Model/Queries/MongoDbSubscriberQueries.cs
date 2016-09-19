using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using Common.MongoDb;
using System.Diagnostics;

namespace SignaloBot.DAL.MongoDb
{
    public class MongoDbSubscriberQueries : ISubscriberQueries<ObjectId>
    {
        //поля
        protected MongoDbConnectionSettings _settings;
        protected ICommonLogger _logger;
        protected SignaloBotMongoDbContext _context;


        //инициализация
        public MongoDbSubscriberQueries(ICommonLogger logger, MongoDbConnectionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
            _context = new SignaloBotMongoDbContext(connectionSettings);
        }

        

        //методы
        public async Task<QueryResult<List<Subscriber<ObjectId>>>> Select(
            SubscribtionParameters parameters, UsersRangeParameters<ObjectId> usersRange)
        {
            var intermidiateResult = new SubscribersIntermidiateResult<ObjectId>();

            if (parameters.SelectFromTopics)
            {
                QueryResult<List<UserTopicSettings<ObjectId>>> topicSubscribers = 
                    await SelectTopics(parameters, usersRange);
                if(topicSubscribers.HasExceptions)
                {
                    return new QueryResult<List<Subscriber<ObjectId>>>(null, true);
                }

                usersRange.FromUserIDs = topicSubscribers.Result.Select(p => p.UserID).ToList();
                intermidiateResult.TopicSubscribers = topicSubscribers.Result;
            }
            
            if (parameters.SelectFromCategories)
            {
                QueryResult<List<UserCategorySettings<ObjectId>>> categorySubscribers =
                    await SelectCategories(parameters, usersRange);
                if (categorySubscribers.HasExceptions)
                {
                    return new QueryResult<List<Subscriber<ObjectId>>>(null, true);
                }

                usersRange.FromUserIDs = categorySubscribers.Result.Select(p => p.UserID).ToList();
                intermidiateResult.CategorySubscribers = categorySubscribers.Result;
            }
  
            return await SelectDeliveryTypes(parameters, usersRange, intermidiateResult);
        }

        public virtual async Task<QueryResult<List<UserTopicSettings<ObjectId>>>> SelectTopics(
            SubscribtionParameters parameters, UsersRangeParameters<ObjectId> usersRange)
        {
            List<UserTopicSettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                    p => p.CategoryID == parameters.CategoryID
                    && p.TopicID == parameters.TopicID
                    && !p.IsDeleted);

                if (usersRange.FromUserIDs != null)
                {
                    filter = Builders<UserTopicSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                            p => usersRange.FromUserIDs.Contains(p.UserID)));
                }

                if (usersRange.UserIDRangeFromExcludingSelf != null)
                {
                    filter = Builders<UserTopicSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                            p => usersRange.UserIDRangeFromExcludingSelf.Value < p.UserID));
                }

                if (usersRange.UserIDRangeToIncludingSelf != null)
                {
                    filter = Builders<UserTopicSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                            p => p.UserID <= usersRange.UserIDRangeToIncludingSelf.Value));
                }

                if (parameters.CheckTopicEnabled)
                {
                    filter = Builders<UserTopicSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserTopicSettings<ObjectId>>.Filter.Where(p => p.IsEnabled));
                }
                
                if (parameters.CheckTopicSendCountNotGreater != null)
                {
                    filter = Builders<UserTopicSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserTopicSettings<ObjectId>>.Filter.Where(
                            p => p.SendCount <= parameters.CheckTopicSendCountNotGreater.Value));
                }

                var projection = Builders<UserTopicSettings<ObjectId>>.Projection
                    .Include(p => p.UserID)
                    .Include(p => p.CategoryID)
                    .Include(p => p.TopicID)
                    .Include(p => p.LastSendDateUtc);
                
                list = await _context.UserTopicSettings.Find(filter)
                    .Project<UserTopicSettings<ObjectId>>(projection)
                    .Limit(usersRange.Limit)
                    .ToListAsync();
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserTopicSettings<ObjectId>>();

            return new QueryResult<List<UserTopicSettings<ObjectId>>>(list, !result);
        }

        public virtual async Task<QueryResult<List<UserCategorySettings<ObjectId>>>> SelectCategories(
            SubscribtionParameters parameters, UsersRangeParameters<ObjectId> usersRange)
        {
            List<UserCategorySettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                    p => p.CategoryID == parameters.CategoryID);

                if (usersRange.GroupID != null)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                        Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                            p => p.GroupID == usersRange.GroupID.Value));
                }

                if (usersRange.FromUserIDs != null)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                        Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                            p => usersRange.FromUserIDs.Contains(p.UserID)));
                }
                
                if (usersRange.UserIDRangeFromExcludingSelf != null)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                        Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                            p => usersRange.UserIDRangeFromExcludingSelf.Value < p.UserID));
                }

                if (usersRange.UserIDRangeToIncludingSelf != null)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                        Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                            p => p.UserID <= usersRange.UserIDRangeToIncludingSelf.Value));
                }

                if (parameters.DeliveryType != null)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                       Builders<UserCategorySettings<ObjectId>>.Filter.Where(p => p.DeliveryType == parameters.DeliveryType.Value));
                }

                if (parameters.CheckCategoryEnabled)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                        Builders<UserCategorySettings<ObjectId>>.Filter.Where(p => p.IsEnabled));
                }
                
                if (parameters.CheckCategorySendCountNotGreater != null)
                {
                    filter = Builders<UserCategorySettings<ObjectId>>.Filter.And(filter,
                        Builders<UserCategorySettings<ObjectId>>.Filter.Where(
                            p => p.SendCount <= parameters.CheckCategorySendCountNotGreater.Value));
                }

                var projection = Builders<UserCategorySettings<ObjectId>>.Projection
                    .Include(p => p.UserID)
                    .Include(p => p.DeliveryType)
                    .Include(p => p.CategoryID)
                    .Include(p => p.LastSendDateUtc);

                list = await _context.UserCategorySettings.Find(filter)
                    .Project<UserCategorySettings<ObjectId>>(projection)
                    .Limit(usersRange.Limit)
                    .ToListAsync();
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

        public virtual async Task<QueryResult<List<Subscriber<ObjectId>>>> SelectDeliveryTypes(
            SubscribtionParameters parameters, UsersRangeParameters<ObjectId> usersRange, SubscribersIntermidiateResult<ObjectId> intermidiateResult)
        {
            List<UserDeliveryTypeSettings<ObjectId>> list = null;
            bool result = false;

            try
            {
                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter
                    .Where(p => p.Address != null);

                if (usersRange.GroupID != null)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                            p => p.GroupID == usersRange.GroupID.Value));
                }

                if (usersRange.FromUserIDs != null)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                            p => usersRange.FromUserIDs.Contains(p.UserID)));
                }

                if (usersRange.UserIDRangeFromExcludingSelf != null)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                            p => usersRange.UserIDRangeFromExcludingSelf.Value < p.UserID));
                }

                if (usersRange.UserIDRangeToIncludingSelf != null)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                            p => p.UserID <= usersRange.UserIDRangeToIncludingSelf.Value));
                }


                if (parameters.DeliveryType != null)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.DeliveryType == parameters.DeliveryType.Value));
                }

                if (parameters.CheckDeliveryTypeEnabled)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.IsEnabled));
                }

                if (parameters.CheckDeliveryTypeLastSendDate)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                            p => p.LastSendDateUtc == null
                            || (p.LastUserVisitUtc != null && p.LastSendDateUtc <= p.LastUserVisitUtc)));
                }

                if (parameters.CheckDeliveryTypeSendCountNotGreater != null)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                            p => p.SendCount <= parameters.CheckDeliveryTypeSendCountNotGreater.Value));
                }

                if (parameters.CheckBlockedOfNDR)
                {
                    filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.And(filter,
                        Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(p => p.IsBlockedOfNDR == false));
                }

                var projection = Builders<UserDeliveryTypeSettings<ObjectId>>.Projection
                    .Include(p => p.UserID)
                    .Include(p => p.DeliveryType)
                    .Include(p => p.Address)
                    .Include(p => p.Language)
                    .Include(p => p.TimeZoneID)
                    .Include(p => p.LastSendDateUtc);
                
                IAsyncCursor<UserDeliveryTypeSettings<ObjectId>> cursor =
                    await _context.UserDeliveryTypeSettings.Find(filter)
                    .Project<UserDeliveryTypeSettings<ObjectId>>(projection)
                    .Limit(usersRange.Limit)
                    .ToCursorAsync();

                list = await FilterDT(cursor, parameters, intermidiateResult);                
                result = true;
            }
            catch (Exception exception)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettings<ObjectId>>();
            
            var subscribers = list.Select(p => new Subscriber<ObjectId>()
            {
                UserID = p.UserID,
                DeliveryType = p.DeliveryType,
                Address = p.Address,
                TimeZoneID = p.TimeZoneID,
                Language = p.Language
            }).ToList();

            return new QueryResult<List<Subscriber<ObjectId>>>(subscribers, !result);
        }

        protected virtual async Task<List<UserDeliveryTypeSettings<ObjectId>>> FilterDT(
            IAsyncCursor<UserDeliveryTypeSettings<ObjectId>> cursor
            , SubscribtionParameters parameters, SubscribersIntermidiateResult<ObjectId> intermidiateResult)
        {
            List<UserDeliveryTypeSettings<ObjectId>> list = new List<UserDeliveryTypeSettings<ObjectId>>();

            while (await cursor.MoveNextAsync())
            {
                List<UserDeliveryTypeSettings<ObjectId>> users = cursor.Current.ToList();

                if (parameters.CheckTopicLastSendDate)
                {
                    users = (from user in users
                             join top in intermidiateResult.TopicSubscribers on
                             new
                             {
                                 UserID = user.UserID,
                             } equals new
                             {
                                 UserID = top.UserID
                             }
                             where top.LastSendDateUtc == null
                                || (user.LastUserVisitUtc != null && top.LastSendDateUtc <= user.LastUserVisitUtc)
                             select user).ToList();
                }

                if (parameters.CheckCategoryLastSendDate)
                {
                    users = (from user in users
                             join cat in intermidiateResult.CategorySubscribers on
                             new
                             {
                                 UserID = user.UserID,
                                 DeliveryType = user.DeliveryType
                             } equals new
                             {
                                 UserID = cat.UserID,
                                 DeliveryType = cat.DeliveryType
                             }
                             where cat.LastSendDateUtc == null
                                || (user.LastUserVisitUtc != null && cat.LastSendDateUtc <= user.LastUserVisitUtc)
                             select user).ToList();
                }

                list.AddRange(users);
            }

            return list;
        }



        //test
        public async Task<long> TestCount()
        {
            var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                p => true);

            var projection = Builders<UserDeliveryTypeSettings<ObjectId>>.Projection
                .Include(p => p.UserID);

            var query = _context.UserDeliveryTypeSettings.Find(filter)
                .Project<UserDeliveryTypeSettings<ObjectId>>(projection);
            
            long count = await query.CountAsync();

            return count;
        }

        public async Task<List<ObjectId>> TestGetRanges(long totalCount, int fragments)
        {
            int range = (int)(totalCount / fragments);
            List<ObjectId> rangeBorders = new List<ObjectId>();
            ObjectId lastUserID = ObjectId.Empty;

            List<TimeSpan> queryTime = new List<TimeSpan>();

            while (true)
            {
                if (rangeBorders.Count > 0)
                {
                    lastUserID = rangeBorders.Last();                    
                }

                var filter = Builders<UserDeliveryTypeSettings<ObjectId>>.Filter.Where(
                    p => p.UserID > lastUserID);
                
                var projection = Builders<UserDeliveryTypeSettings<ObjectId>>.Projection
                    .Include(p => p.UserID);

                Stopwatch sw = Stopwatch.StartNew();

                //string explain = _context.UserDeliveryTypeSettings.ExplainFind<UserDeliveryTypeSettings<ObjectId>, UserDeliveryTypeSettings<ObjectId>>(
                //    MongoDB.Driver.Core.Operations.ExplainVerbosity.ExecutionStats
                //    , filter, new FindOptions<UserDeliveryTypeSettings<ObjectId>, UserDeliveryTypeSettings<ObjectId>>()
                //    {
                //        Limit = 1,
                //        Sort = Builders<UserDeliveryTypeSettings<ObjectId>>.Sort.Ascending(f => f.UserID),
                //        Projection = projection
                //    }).Result.ToJsonIntended();
                //break;

                UserDeliveryTypeSettings<ObjectId> borderEntity = await _context.UserDeliveryTypeSettings
                    .Find(filter)
                    .SortBy(p => p.UserID)
                    .Project<UserDeliveryTypeSettings<ObjectId>>(projection)
                    .Skip(range)
                    .Limit(1)
                    .FirstOrDefaultAsync();

                TimeSpan time = sw.Elapsed;

                queryTime.Add(time);

                if (borderEntity == null)
                {
                    break;
                }
                else
                {
                    rangeBorders.Add(borderEntity.UserID);
                }
            }

            return rangeBorders;
        }

    }
}
