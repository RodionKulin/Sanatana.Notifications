using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.Composing
{
    public interface ISubscribersFetcher<TKey>
        where TKey : struct
    {
        ComposeResult<Subscriber<TKey>> Select(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent);
    }
}