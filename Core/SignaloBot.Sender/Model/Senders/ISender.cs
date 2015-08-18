using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders
{
    public interface ISender<T> : IDisposable
        where T : IMessage
    {
        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        SendResult Send(T message);

        /// <summary>
        /// Проверить доступность сервиса. Вызывается в случае неудачной попытки отправки сообщения. Если проверка не реализована, то следует возвращать null.
        /// </summary>
        /// <returns></returns>
        SenderAvailability CheckAvailability();
    }
}
