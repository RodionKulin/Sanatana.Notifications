using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using System;

namespace SignaloBot.Sender.Queue
{
    public interface IMessageQueue<T> : IDisposable
        where T : IMessage
    {
        int MemoryQueueLength { get; }
        void QueryStorage(System.Collections.Generic.List<int> deliveryTypes);
        T DequeueNext();
        void ApplyResult(T message, SendResult sendResult, SenderAvailability senderAvailable);
    }
}
