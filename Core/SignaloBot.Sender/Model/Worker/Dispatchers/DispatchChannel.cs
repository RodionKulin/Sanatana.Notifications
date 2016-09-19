using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Senders.FailCounter;
using SignaloBot.Sender.Senders.LimitManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders
{
    public class DispatchChannel<TKey> : IDisposable
        where TKey : struct
    {
        //свойства
        public int DeliveryType { get; set; }

        /// <summary>
        /// Отправитель сообщений
        /// </summary>
        public IDispatcher<TKey> Sender { get; set; }
        
        public ILimitManager LimitManager { get; set; }

        public IFailCounter FailCounter { get; set; }

        /// <summary>
        /// Время снятия лимитов на отправку через этот канал.
        /// </summary>
        internal DateTime NextLimitsEndTimeUtc { get; set; }

        /// <summary>
        /// Доступное количество доставок до достижения лимита
        /// </summary>
        internal int AvailableLimitCapacity { get; set; }



        //зависимые свойства
        internal virtual bool IsActive
        {
            get
            {
                return !FailCounter.IsPenaltyActive
                    && NextLimitsEndTimeUtc <= DateTime.UtcNow;
            }
        }



        //инициализация
        public DispatchChannel()
        {
            LimitManager = new NoLimitManager();
            FailCounter = new ProgressiveFailCounter();
        }
        public DispatchChannel(int deliveryType, IDispatcher<TKey> sender)
            : this()
        {
            DeliveryType = deliveryType;
            Sender = sender;
        }

        

        //методы
        public virtual bool MatchChannel(SignalDispatchBase<TKey> item)
        {
            return item.DeliveryType == DeliveryType;
        }

        public virtual DispatcherAvailability ApplyResult(ProcessingResult result)
        {
            if(result == ProcessingResult.Success)
            {
                AvailableLimitCapacity--;
                LimitManager.InsertTime();
                FailCounter.Success();
                return DispatcherAvailability.Available;
            }
            else if (result == ProcessingResult.Fail)
            {
                DispatcherAvailability senderAvailable = CheckAvailability();
                AvailableLimitCapacity--;
                LimitManager.InsertTime();

                if (senderAvailable == DispatcherAvailability.Available)
                {
                    FailCounter.Success();
                }
                else if (senderAvailable == DispatcherAvailability.NotAvailable)
                {
                    FailCounter.Fail();
                }

                return senderAvailable;
            }
            else
            {
                return DispatcherAvailability.NotChecked;
            }
        }
        
        /// <summary>
        /// Проверить доступность канала отправки сообщений.
        /// </summary>
        /// <returns></returns>
        protected virtual DispatcherAvailability CheckAvailability()
        {
            DispatcherAvailability senderAvailable = Sender.CheckAvailability();

            if (senderAvailable != DispatcherAvailability.NotChecked)
            {
                AvailableLimitCapacity--;
                LimitManager.InsertTime();
            }

            return senderAvailable;
        }
               


        //IDispose
        public virtual void Dispose()
        {
            if(Sender != null)
                Sender.Dispose();

            if(LimitManager != null)
                LimitManager.Dispose();
        }
    }
}
