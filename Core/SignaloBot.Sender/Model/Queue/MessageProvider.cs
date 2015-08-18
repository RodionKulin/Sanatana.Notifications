using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue.InsertNotifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue
{
    public class MessageProvider<T> : IDisposable
        where T : IMessage
    {
        //свойства настройки
        public IMessageQueue<T> MessageQueue { get; set; }

        public IStorageInsertNotifier StorageInsertNotifier { get; set; }

        /// <summary>
        /// Период обращения к хранилищу для поиска новых сообщений в очереди и отправки отложенных сообщений.
        /// </summary>
        public TimeSpan StorageQueryPeriod { get; set; }
        

        //свойства состояния
        /// <summary>
        /// Время последнего обращения к хранилищу
        /// </summary>
        internal DateTime LastStorageQueryTimeUtc { get; set; }

        internal DateTime NextStorageQueryTimeUtc
        {
            get
            {
                return LastStorageQueryTimeUtc + StorageQueryPeriod;
            }
        }

        internal List<int> LastQueryDeliveryTypes { get; set; }


        //инициализация
        public MessageProvider()
        {
            StorageQueryPeriod = SenderConstants.STORAGE_QUERY_PERIOD_DEFAULT;
        }



        //IDisposable 
        public virtual void Dispose()
        {
            if (MessageQueue != null)
                MessageQueue.Dispose();

            if (StorageInsertNotifier != null)
                StorageInsertNotifier.Dispose();
        }
    }
}
