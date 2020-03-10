using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling.Channels;
using System;

namespace Sanatana.Notifications.DispatchHandling.Interrupters
{
    public interface IInterrupter<TKey>
        where TKey : struct
    {
        void Success(SignalDispatch<TKey> dispatch);

        void Fail(SignalDispatch<TKey> dispatch, DispatcherAvailability availability);

        DateTime? GetTimeoutEndUtc();
    }
}
