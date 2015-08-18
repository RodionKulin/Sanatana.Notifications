using SignaloBot.DAL;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Queries.Sender;

namespace SignaloBot.Sender.Queue
{
    public class MessageQueue<T> : IMessageQueue<T>
        where T : IMessage
    {
        //поля
        protected Queue<T> _messageQueue;
        protected IQueueQueries<T> _storageQueries;
        protected object _queueLock;


        //свойства
        /// <summary>
        /// Количество загруженных в память сообщений.
        /// </summary>
        /// <returns></returns>
        public virtual int MemoryQueueLength
        {
            get
            {
                return _messageQueue.Count;
            }
        }



        //свойства настройки
        /// <summary>
        /// Максимальное количество попыток 
        /// </summary>
        public int MaxDeliveryFailedAttempts { get; set; }

        /// <summary>
        /// Период повторной отправки сообщения после неуспешной попытки.
        /// </summary>
        public TimeSpan FailedAttemptRetryPeriod { get; set; }

        /// <summary>
        /// Количество сообщений получаемых из хранилища за 1 запрос.
        /// </summary>
        public int StorageMessageQueryCount { get; set; }



        //инициализация
        public MessageQueue(IQueueQueries<T> storageQueries)
        {
            _storageQueries = storageQueries;
            _messageQueue = new Queue<T>();
            _queueLock = new object();
            MaxDeliveryFailedAttempts = SenderConstants.MAX_DELIVERY_FAILED_ATTEMPTS_DEFAULT;
            FailedAttemptRetryPeriod = SenderConstants.FAILED_ATTEMPT_RETRY_PERIOD_DEFAULT;
            StorageMessageQueryCount = SenderConstants.STORAGE_MESSAGE_QUERY_COUNT_DEFAULT;
        }


        //методы
        /// <summary>
        /// Получить следующее сообщение из очереди в памяти.
        /// </summary>
        /// <returns></returns>
        public virtual T DequeueNext()
        {
            lock (_queueLock)
            {
                return _messageQueue.Dequeue();
            }
        }

        /// <summary>
        /// Применить результаты отправки сообщения к хранилищу сообщений.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sendResult"></param>
        /// <param name="senderAvailable"></param>
        public virtual void ApplyResult(T message, SendResult sendResult, SenderAvailability senderAvailable)
        {
            if (sendResult == SendResult.Success)
            {
                _storageQueries.Delete(message);
            }
            else if (sendResult == SendResult.Fail)
            {
                if (senderAvailable == SenderAvailability.Available
                    || senderAvailable == SenderAvailability.NotChecked)
                {
                    message.FailedAttempts++;
                }

                TimeSpan failedAttemptRetryPeriod = FailedAttemptRetryPeriod > TimeSpan.Zero
                    ? FailedAttemptRetryPeriod
                    : TimeSpan.Zero;

                message.SendDateUtc = DateTime.UtcNow.Add(failedAttemptRetryPeriod);
                message.IsDelayed = false;

                _storageQueries.Update(message);
            }
        }
        
        /// <summary>
        /// Запросить из хранилища сообщения, которые нужно отправить.
        /// </summary>
        /// <param name="entitiesNumberToQuery"></param>
        public virtual void QueryStorage(List<int> deliveryTypes)
        {
            int maxMessageDeliveryFailedAttempts = MaxDeliveryFailedAttempts < 1
                ? 1
                : MaxDeliveryFailedAttempts;

            int storageMessageQueryCount = StorageMessageQueryCount < 1
                ? 1
                : StorageMessageQueryCount;

            List<T> messageList = _storageQueries.Select(storageMessageQueryCount, deliveryTypes, maxMessageDeliveryFailedAttempts);
                
            foreach (T item in messageList)
            {
                _messageQueue.Enqueue(item);
            }
        }


        //IDisposable
        public virtual void Dispose()
        {
            _storageQueries.Dispose();
        }
    }
}
