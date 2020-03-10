using System.Collections.Generic;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.EventsHandling
{
    public interface IDispatchBuilder<TKey>
        where TKey : struct
    {
        EventHandleResult<SignalDispatch<TKey>> Build(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers);
    }
}