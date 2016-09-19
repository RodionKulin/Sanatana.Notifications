using Common.Utility;
using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Statistics
{
    public interface IStatisticsCollector<TKey> : IDisposable
        where TKey : struct
    {
        void HubSwitched(bool switchedOn);

        void EventTransferred(SignalEventBase<TKey> item);

        void EventStorageQueried(TimeSpan time, QueryResult<List<SignalEventBase<TKey>>> items);

        void DispatchStorageQueried(TimeSpan time, QueryResult<List<SignalDispatchBase<TKey>>> items);

        void DispatchesComposed(SignalEventBase<TKey> item, TimeSpan time, ProcessingResult composeResult);

        void DispatchSended(SignalDispatchBase<TKey> item
            , TimeSpan time, ProcessingResult sendResult, DispatcherAvailability senderAvailability);
    }
}
