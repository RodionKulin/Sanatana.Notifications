using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventTracking;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Dispatching;
using System;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.Queues
{
    public interface IDispatchQueue<TKey> : IQueue<SignalDispatch<TKey>>, IRegularJob
        where TKey : struct
    {
    }
}