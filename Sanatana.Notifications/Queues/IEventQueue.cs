using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Queues
{
    public interface IEventQueue<TKey> : IQueue<SignalEvent<TKey>>, IRegularJob
        where TKey : struct
    {
    }
}
