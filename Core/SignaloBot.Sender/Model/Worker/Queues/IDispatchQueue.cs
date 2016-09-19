using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using System;

namespace SignaloBot.Sender.Queue
{
    public interface IDispatchQueue<TKey> : IDisposable
        where TKey : struct
    {
        int ReturnToStorageAfterItemsCount { get; set; }

        void OnTick(System.Collections.Generic.List<int> deliveryTypes, Statistics.IStatisticsCollector<TKey> stats);
        void ReturnAll();
        void Append(System.Collections.Generic.List<SignalDispatchBase<TKey>> items, bool isStored);
        SignalWrapper<SignalDispatchBase<TKey>> DequeueNext(System.Collections.Generic.List<int> deliveryTypes);
        void ApplyResult(SignalWrapper<SignalDispatchBase<TKey>> item, ProcessingResult result);
        bool CheckIsEmpty(System.Collections.Generic.List<int> deliveryTypes);
        int CountQueueItems();
    }
}
