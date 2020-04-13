using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalBounceQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<SignalBounce<TKey>> items);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">0-based page index</param>
        /// <param name="pageSize"></param>
        /// <param name="receiverSubscriberIds"></param>
        /// <returns></returns>
        Task<TotalResult<List<SignalBounce<TKey>>>> SelectPage(
             int pageIndex, int pageSize, List<TKey> receiverSubscriberIds = null);
        Task Delete(List<TKey> receiverSubscriberIds);
        Task Delete(List<SignalBounce<TKey>> items);
    }
}