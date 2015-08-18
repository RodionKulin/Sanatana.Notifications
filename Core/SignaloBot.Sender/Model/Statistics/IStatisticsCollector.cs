using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Statistics
{
    public interface IStatisticsCollector<T> : IDisposable
        where T : IMessage
    {
        void DispatcherSwitched(bool switchedOn);

        void StorageQueried(MessageProvider<T> messageProvider, TimeSpan time);

        void MessageProcessed(SendChannel<T> sendChannel, T message, SendResult sendResult, TimeSpan time);
    }
}
