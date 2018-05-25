using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Timers.Switchables;

namespace Sanatana.Notifications.Monitoring
{
    public class ConsoleMonitor<TKey> : IMonitor<TKey>
            where TKey : struct
    {
        //methods
        public void SenderSwitched(SwitchState state)
        {
            Console.WriteLine(MonitorMessages.SenderSwitched
                , DateTime.Now.ToLongTimeString(), state);
        }
        
        public void EventTransferred(SignalEvent<TKey> signalEvent)
        {
            Console.WriteLine(MonitorMessages.EventTransferred
               , DateTime.Now.ToLongTimeString(), signalEvent.CategoryId);
        }

        public void DispatchTransferred(SignalDispatch<TKey> signalDispatch)
        {
            Console.WriteLine(MonitorMessages.DispatchTransferred
               , DateTime.Now.ToLongTimeString(), signalDispatch.CategoryId);
        }

        public void EventPersistentStorageQueried(TimeSpan time, List<SignalEvent<TKey>> events)
        {
            int receivedItemsCount = events?.Count ?? 0;
            Console.WriteLine(MonitorMessages.EventPersistentStorageQueried
                , DateTime.Now.ToLongTimeString(), time, receivedItemsCount);
        }

        public void DispatchPersistentStorageQueried(TimeSpan time, List<SignalDispatch<TKey>> dispatches)
        {
            int receivedItemsCount = dispatches?.Count ?? 0;
            Console.WriteLine(MonitorMessages.DispatchPersistentStorageQueried
                , DateTime.Now.ToLongTimeString(), time, receivedItemsCount);
        }

        public void DispatchesComposed(SignalEvent<TKey> item, TimeSpan time, ProcessingResult composeResult, List<SignalDispatch<TKey>> dispatches)
        {
            Console.WriteLine(MonitorMessages.DispatchesComposed
                , DateTime.Now.ToLongTimeString(), dispatches.Count, composeResult, time);
        }

        public void DispatchSent(SignalDispatch<TKey> item, TimeSpan time, ProcessingResult sendResult, DispatcherAvailability senderAvailability)
        {
            Console.WriteLine(MonitorMessages.DispatchSent
               , DateTime.Now.ToLongTimeString(), sendResult, time);
        }
    }
}
