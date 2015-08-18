using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Statistics;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Dispatcher
{
    public class MessageDispatcher<T> : IDisposable
        where T : IMessage
    {
        //поля
        private DispatcherWorker<T> _dispatcherWorker;
        

        //свойства настройки
        public ICommonLogger Logger { get; set; }

        public List<MessageProvider<T>> MessageProviders { get; set; }

        public SenderProvider<T> SenderProvider { get; set; }

        public IStatisticsCollector<T> StatisticsCollector { get; set; }

        /// <summary>
        /// Максимальное число параллельных доставок. По умолчанию равняется числу ядер процессора. 
        /// </summary>
        public int MaxParallelDeliveries { get; set; }

                
        //свойства состояния
        /// <summary>
        /// Статус диспетчера рассылки
        /// </summary>
        public bool IsStarted { get; private set; }



        //инициализация
        public MessageDispatcher()
        {
            MessageProviders = new List<MessageProvider<T>>();
            SenderProvider = new SenderProvider<T>();
            MaxParallelDeliveries = Environment.ProcessorCount;

            _dispatcherWorker = new DispatcherWorker<T>(this);
        }



        //методы
        /// <summary>
        /// Запустить рассылку
        /// </summary>
        public virtual void Start()
        {
            if (IsStarted)
            {
                return;
            }

            IsStarted = true;

            if (StatisticsCollector != null)
            {
                StatisticsCollector.DispatcherSwitched(true);
            }

            _dispatcherWorker.Start();
        }

        /// <summary>
        /// Остановить рассылку
        /// </summary>
        public virtual void Stop()
        {
            if (!IsStarted)
            {
                return;
            }

            if (StatisticsCollector != null)
            {
                StatisticsCollector.DispatcherSwitched(false);
            }

            IsStarted = false;

            _dispatcherWorker.Stop();
        }



        //IDisposable  
        public virtual void Dispose()
        {
            Stop();

            if (SenderProvider != null)
            {
                SenderProvider.Dispose();
            }

            foreach (MessageProvider<T> messageProvider in MessageProviders)
            {
                if (messageProvider != null)
                {
                    messageProvider.Dispose();
                }
            }

            if (StatisticsCollector != null)
            {
                StatisticsCollector.Dispose();
            }
        }
    }
}
