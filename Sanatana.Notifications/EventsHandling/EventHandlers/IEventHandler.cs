using System;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.EventsHandling
{
    public interface IEventHandler<TKey> 
        where TKey : struct
    {
        int? EventHandlerId { get; set; }
        EventHandleResult<SignalDispatch<TKey>> ProcessEvent(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent);
    }
}