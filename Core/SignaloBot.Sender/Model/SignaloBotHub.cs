using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Statistics;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL;
using SignaloBot.Sender.Resources;
using SignaloBot.Sender.Composers;
using SignaloBot.Sender.Service;

namespace SignaloBot.Sender
{
    public class SignaloBotHub<TKey> : IDisposable
        where TKey : struct
    {
        //поля
        private SignaloBotWorker<TKey> _dispatcherWorker;
        private int _maxParallelSenders;
        private int _maxParallelComposers;



        //свойства настройки
        public virtual ICommonLogger Logger { get; set; }

        public virtual SignalServiceSelfHost<TKey> ServiceHost { get; set; }

        public virtual List<IEventQueue<TKey>> EventQueues { get; set; }

        public virtual ComposerBase<TKey> Composer { get; set; }

        public virtual List<IDispatchQueue<TKey>> DispatchQueues { get; set; }

        public virtual List<DispatchChannel<TKey>> Senders { get; set; }

        public virtual IStatisticsCollector<TKey> StatisticsCollector { get; set; }

        /// <summary>
        /// Максимальное число параллельных доставок. По умолчанию равняется числу ядер процессора. 
        /// </summary>
        public virtual int MaxParallelSenders
        {
            get
            {
                return _maxParallelSenders < 1
                    ? 1
                    : _maxParallelSenders;
            }
            set
            {
                _maxParallelSenders = value;
            }
        }

        /// <summary>
        /// Максимальное число параллельных составлений. По умолчанию равняется числу ядер процессора. 
        /// </summary>
        public virtual int MaxParallelComposers
        {
            get
            {
                return _maxParallelComposers < 1
                    ? 1
                    : _maxParallelComposers;
            }
            set
            {
                _maxParallelComposers = value;
            }
        }



        //свойства состояния
        public virtual SwitchState State { get; internal set; }



        //инициализация
        public SignaloBotHub()
        {
            MaxParallelSenders = Environment.ProcessorCount;
            MaxParallelComposers = Environment.ProcessorCount;

            EventQueues = new List<IEventQueue<TKey>>();
            DispatchQueues = new List<IDispatchQueue<TKey>>();
            Senders = new List<DispatchChannel<TKey>>();

            _dispatcherWorker = new SignaloBotWorker<TKey>(this);
        }



        //методы
        /// <summary>
        /// Запустить рассылку
        /// </summary>
        public virtual void Start()
        {
            if (State == SwitchState.Started)
            {
                return;
            }
            State = SwitchState.Started;

            if (StatisticsCollector != null)
            {
                StatisticsCollector.HubSwitched(true);
            }

            if (ServiceHost != null)
            {
                ServiceHost.Start();
            }
            _dispatcherWorker.Start();
        }

        /// <summary>
        /// Остановить рассылку
        /// </summary>
        public virtual void Stop(bool blockThreadToWait, TimeSpan? timeout = null)
        {
            if (State != SwitchState.Started)
            {
                return;
            }
            
            _dispatcherWorker.Stop(blockThreadToWait, timeout);

            if (StatisticsCollector != null)
            {
                StatisticsCollector.HubSwitched(false);
            }
        }

        internal List<int> GetActiveSendersTypes(bool checkLimitCapacity)
        {
            if (Senders == null)
                return new List<int>();

            IEnumerable<DispatchChannel<TKey>> activeSenders = Senders
                .Where(p => p.IsActive);

            if(checkLimitCapacity)
            {
                activeSenders = activeSenders.Where(p => p.AvailableLimitCapacity > 0);
            }

            return activeSenders
                .Select(p => p.DeliveryType)
                .Distinct()
                .ToList();
        }
        


        //IDisposable  
        public virtual void Dispose()
        {
            if(EventQueues != null)
            {
                foreach (IEventQueue<TKey> eventQueue in EventQueues)
                {
                    eventQueue.Dispose();
                }
            }

            if (DispatchQueues != null)
            {
                foreach (IDispatchQueue<TKey> dispatchQueue in DispatchQueues)
                {
                    dispatchQueue.Dispose();
                }
            }

            if (Composer != null)
            {
                Composer.Dispose();
            }
                
            if(Senders != null)
            {
                foreach (DispatchChannel<TKey> sender in Senders)
                {
                    sender.Dispose();
                }
            }

            if (StatisticsCollector != null)
            {
                StatisticsCollector.Dispose();
            }
        }
    }
}
