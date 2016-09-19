using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders
{
    public interface IDispatcher<TKey> : IDisposable
        where TKey : struct
    {
        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        ProcessingResult Send(SignalDispatchBase<TKey> item);

        /// <summary>
        /// Проверить доступность сервиса. Вызывается в случае неудачной попытки отправки сообщения.
        /// </summary>
        /// <returns></returns>
        DispatcherAvailability CheckAvailability();
    }
}
