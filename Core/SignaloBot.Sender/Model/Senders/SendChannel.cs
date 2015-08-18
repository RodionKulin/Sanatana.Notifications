using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Senders.FailCounter;
using SignaloBot.Sender.Senders.LimitManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders
{
    public class SendChannel<T> : IDisposable
        where T : IMessage
    {
        //свойства
        public int DeliveryType { get; set; }

        /// <summary>
        /// Отправитель сообщений
        /// </summary>
        public ISender<T> Sender { get; set; }
        
        public ILimitManager LimitManager { get; set; }

        public IFailCounter FailCounter { get; set; }

        /// <summary>
        /// Время снятия лимитов на отправку через этот канал.
        /// </summary>
        public DateTime NextLimitsEndTimeUtc { get; set; }

        /// <summary>
        /// Доступное количество доставок до достижения лимита
        /// </summary>
        public int AvailableLimitCapacity { get; set; }



        //зависимые свойства
        public virtual bool IsActive
        {
            get
            {
                return !FailCounter.IsPenaltyActive
                    && NextLimitsEndTimeUtc <= DateTime.UtcNow;
            }
        }



        //методы
        /// <summary>
        /// Отметить удачную попытку отправки сообщения.
        /// </summary>
        public virtual void MarkSuccessfulAttempt()
        {
            AvailableLimitCapacity--;

            LimitManager.InsertTime();
            
            FailCounter.Success();
        }

        /// <summary>
        /// Отметить неудачную попытку отправки сообщения.
        /// </summary>
        /// <returns>Статус доступности канала отправки сообщений.</returns>
        public virtual SenderAvailability MarkFailedAttempt()
        {
            SenderAvailability senderAvailable = CheckAvailability();

            AvailableLimitCapacity--;

            LimitManager.InsertTime();

            if (senderAvailable == SenderAvailability.Available)
            {
                FailCounter.Success();
            }
            else if (senderAvailable == SenderAvailability.NotAvailable)
            {
                FailCounter.Fail();
            }

            return senderAvailable;
        }

        /// <summary>
        /// Проверить доступность канала отправки сообщений.
        /// </summary>
        /// <returns></returns>
        protected virtual SenderAvailability CheckAvailability()
        {
            SenderAvailability senderAvailable = Sender.CheckAvailability();

            if (senderAvailable != SenderAvailability.NotChecked)
            {
                AvailableLimitCapacity--;

                LimitManager.InsertTime();
            }

            return senderAvailable;
        }
               


        //IDispose
        public virtual void Dispose()
        {
            Sender.Dispose();

            LimitManager.Dispose();
        }
    }
}
