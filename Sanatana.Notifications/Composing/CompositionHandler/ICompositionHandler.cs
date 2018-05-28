using System;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.Composing
{
    public interface ICompositionHandler<TKey> 
        where TKey : struct
    {
        int? CompositionHandlerId { get; set; }
        ComposeResult<SignalDispatch<TKey>> ProcessEvent(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent);
    }
}