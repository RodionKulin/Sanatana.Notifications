using System.Collections.Generic;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.Composing
{
    public interface IDispatchBuilder<TKey>
        where TKey : struct
    {
        ComposeResult<SignalDispatch<TKey>> Build(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers);
    }
}