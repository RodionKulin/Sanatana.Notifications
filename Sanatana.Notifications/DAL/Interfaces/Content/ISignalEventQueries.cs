using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalEventQueries<TKey> : ISignalQueries<SignalEvent<TKey>>
        where TKey : struct
    {
        Task<List<SignalEvent<TKey>>> Select(int count, int maxFailedAttempts);
    }
}
