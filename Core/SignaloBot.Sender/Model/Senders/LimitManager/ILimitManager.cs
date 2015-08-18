using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders.LimitManager
{
    public interface ILimitManager : IDisposable
    {
        /// <summary>
        /// Увеличить счётчик отправленных сообщений.
        /// </summary>
        void InsertTime();

                
        /// <summary>
        /// Получить время до снятия лимитов. В случае ошибки возвращать DateTime.UtcNow.
        /// </summary>
        /// <returns></returns>
        DateTime GetLimitsEndTimeUtc();


        /// <summary>
        /// Получить доступное число доставок. В случае ошибки возвращать 0.
        /// </summary>
        /// <returns></returns>
        int GetLimitCapacity();
    }
}
