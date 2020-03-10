using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.EventsHandling
{
    public interface ISubscribersFetcher<TKey>
        where TKey : struct
    {
        EventHandleResult<Subscriber<TKey>> Select(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent);
    }
}