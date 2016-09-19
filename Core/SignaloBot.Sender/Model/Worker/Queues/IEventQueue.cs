using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Senders;
using System;

namespace SignaloBot.Sender.Queue
{
    public interface IEventQueue<TKey> : IDisposable
        where TKey : struct
    {
        int ReturnToStorageAfterItemsCount { get; set; }

        void OnTick(Statistics.IStatisticsCollector<TKey> stats);
        void ReturnAll();
        void Append(System.Collections.Generic.List<SignalEventBase<TKey>> items, bool isStored);
        void Append(SignalWrapper<SignalEventBase<TKey>> item);        
        void ApplyResult(SignalWrapper<SignalEventBase<TKey>> item, ProcessingResult result);
        int CountQueueItems();
        SignalWrapper<SignalEventBase<TKey>> DequeueNext();
    }
}