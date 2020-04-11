using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Models;

namespace Sanatana.Notifications.EventsHandling
{
    public interface IScheduler<TKey>
        where TKey : struct
    {
        ProcessingResult SetSendingTime(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , List<Subscriber<TKey>> subscribers, List<SignalDispatch<TKey>> dispatches);
    }
}
