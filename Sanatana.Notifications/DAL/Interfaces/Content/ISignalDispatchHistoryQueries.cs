using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalDispatchHistoryQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<SignalDispatch<TKey>> items);

        Task Select(int pageNumber, int pageSize);

        Task Update(List<SignalDispatch<TKey>> items);

        Task Delete(List<SignalDispatch<TKey>> items);
    }
}
