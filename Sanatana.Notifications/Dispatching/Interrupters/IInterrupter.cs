using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Dispatching.Channels;
using System;

namespace Sanatana.Notifications.Dispatching.Interrupters
{
    public interface IInterrupter<TKey>
        where TKey : struct
    {
        void Success(SignalDispatch<TKey> dispatch);

        void Fail(SignalDispatch<TKey> dispatch, DispatcherAvailability availability);

        DateTime? GetTimeoutEndUtc();
    }
}
