using System;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.EventsHandling
{
    public interface ICompositionHandler<TKey> 
        where TKey : struct
    {
        int? CompositionHandlerId { get; set; }
        EventHandleResult<SignalDispatch<TKey>> ProcessEvent(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent);
    }
}