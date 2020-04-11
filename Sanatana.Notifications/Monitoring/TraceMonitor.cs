using Sanatana.Timers.Switchables;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Models;

namespace Sanatana.Notifications.Monitoring
{
    /// <summary>
    /// Writes output to the Immediate Window
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TraceMonitor<TKey> : IMonitor<TKey>
        where TKey : struct
    {
        //methods
        public void SenderSwitched(SwitchState state)
        {
            string message = string.Format(MonitorMessages.SenderSwitched
                , DateTime.Now.ToLongTimeString(), state);
            Trace.WriteLine(message);
        }

        public void EventReceived(SignalEvent<TKey> signalEvent)
        {
            string message = string.Format(MonitorMessages.EventTransferred
               , DateTime.Now.ToLongTimeString(), signalEvent.EventKey);
            Trace.WriteLine(message);
        }

        public void DispatchTransferred(SignalDispatch<TKey> signalDispatch)
        {
            string message = string.Format(MonitorMessages.DispatchTransferred
              , DateTime.Now.ToLongTimeString(), signalDispatch.CategoryId);
            Trace.WriteLine(message);
        }

        public void EventPersistentStorageQueried(TimeSpan time, List<SignalEvent<TKey>> events)
        {
            int receivedItemsCount = events?.Count ?? 0;
            string message = string.Format(MonitorMessages.EventPersistentStorageQueried
               , DateTime.Now.ToLongTimeString(), time, receivedItemsCount);
            Trace.WriteLine(message);
        }

        public void DispatchPersistentStorageQueried(TimeSpan time, List<SignalDispatch<TKey>> dispatches)
        {
            int receivedItemsCount = dispatches?.Count ?? 0;
            string message = string.Format(MonitorMessages.DispatchPersistentStorageQueried
               , DateTime.Now.ToLongTimeString(), time, receivedItemsCount);
            Trace.WriteLine(message);
        }

        public void DispatchesComposed(SignalEvent<TKey> item, TimeSpan time, ProcessingResult composeResult, List<SignalDispatch<TKey>> dispatches)
        {
            string message = string.Format(MonitorMessages.DispatchesComposed
               , DateTime.Now.ToLongTimeString(), dispatches.Count, composeResult, time);
            Trace.WriteLine(message);
        }

        public void DispatchSent(SignalDispatch<TKey> item, TimeSpan time, ProcessingResult sendResult, DispatcherAvailability senderAvailability)
        {
            string message = string.Format(MonitorMessages.DispatchSent
              , DateTime.Now.ToLongTimeString(), sendResult, time);
            Trace.WriteLine(message);
        }
    }
}
