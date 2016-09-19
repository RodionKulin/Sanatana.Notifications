using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.DAL
{
    public interface ISubscriberQueries<TKey>
        where TKey : struct
    {
        Task<QueryResult<List<Subscriber<TKey>>>> Select(
            SubscribtionParameters parameters, UsersRangeParameters<TKey> usersRange);

        Task<QueryResult<List<UserTopicSettings<TKey>>>> SelectTopics(
            SubscribtionParameters parameters, UsersRangeParameters<TKey> usersRange);

        Task<QueryResult<List<UserCategorySettings<TKey>>>> SelectCategories(
            SubscribtionParameters parameters, UsersRangeParameters<TKey> usersRange);

        Task<QueryResult<List<Subscriber<TKey>>>> SelectDeliveryTypes(
            SubscribtionParameters parameters, UsersRangeParameters<TKey> usersRange
            , SubscribersIntermidiateResult<TKey> intermidiateResult);
    }
}