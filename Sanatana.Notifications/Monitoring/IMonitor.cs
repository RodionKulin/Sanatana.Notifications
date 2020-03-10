using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.DispatchHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Timers.Switchables;

namespace Sanatana.Notifications.Monitoring
{
    public interface IMonitor<TKey>
        where TKey : struct
    {
        void SenderSwitched(SwitchState state);

        void EventTransferred(SignalEvent<TKey> signalEvent);

        void DispatchTransferred(SignalDispatch<TKey> signalDispatch);

        void EventPersistentStorageQueried(TimeSpan time, List<SignalEvent<TKey>> events);

        void DispatchPersistentStorageQueried(TimeSpan time, List<SignalDispatch<TKey>> dispatches);

        void DispatchesComposed(SignalEvent<TKey> item
            , TimeSpan time, ProcessingResult composeResult, List<SignalDispatch<TKey>> dispatches);

        void DispatchSent(SignalDispatch<TKey> item
            , TimeSpan time, ProcessingResult sendResult, DispatcherAvailability dispatcherAvailability);
    }
}
