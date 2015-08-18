using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Dispatcher
{
    internal class DispatcherWorker<T>
        where T : IMessage
    {
        //поля
        protected NonReentrantTimer _dispatcherTimer;
        protected MessageDispatcher<T> _dispatcherContext;
        


        //инициализация
        public DispatcherWorker(MessageDispatcher<T> dispatcherContext)
        {
            _dispatcherTimer = new NonReentrantTimer(DispatcherTimerCallback
                , SenderConstants.DISPATCHER_TIMER_INTERVAL, intervalFromCallbackStarted: true);
            
            _dispatcherContext = dispatcherContext;
        }



        //запуск таймера
        /// <summary>
        /// Запустить рассылку
        /// </summary>
        public virtual void Start()
        {
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Остановить рассылку
        /// </summary>
        public virtual void Stop()
        {
            _dispatcherTimer.Stop();

            foreach (MessageProvider<T> messageProvider in _dispatcherContext.MessageProviders)
            {
                if (messageProvider.StorageInsertNotifier != null)
                {
                    messageProvider.StorageInsertNotifier.StopMonitor();
                }
            }
        }

        /// <summary>
        /// Отправить все сообщения в очереди и получить новые из базы, пока позволяют лимиты.
        /// </summary>
        protected virtual bool DispatcherTimerCallback()
        {
            bool canStartDequeue = CanStartDequeue();
            if (canStartDequeue)
            {
                StartDequeue();
            }

            //вернуть продолжать ли работу таймера
            return _dispatcherContext.IsStarted;
        }



        //проверка состояния для начала цикла доставки
        protected virtual bool CanStartDequeue()
        {
            //проверить ограничения
            List<SendChannel<T>> activeSenders = _dispatcherContext.SenderProvider.GetActiveSenders();
            if (activeSenders.Count == 0)
            {
                return false;
            }

            //проверить причины для начала обработки очереди сообщений
            bool isMessageProviderReady = _dispatcherContext.MessageProviders
                .Any(p => CanStartDequeueMessageProvider(p));

            return isMessageProviderReady && _dispatcherContext.IsStarted;
        }

        protected virtual bool CanStartDequeueMessageProvider(MessageProvider<T> messageProvider)
        {
            bool queueNotEmpty = messageProvider.MessageQueue.MemoryQueueLength > 0;

            bool newStorageMessages = messageProvider.StorageInsertNotifier != null
                && messageProvider.StorageInsertNotifier.HasUpdates;

            bool doScheduledQuery = messageProvider.NextStorageQueryTimeUtc <= DateTime.UtcNow;

            List<int> activeSenders = _dispatcherContext.SenderProvider.GetActiveSenderTypes();
            bool hasUnqueriedDeliveryTypes = messageProvider.LastQueryDeliveryTypes == null 
                || activeSenders.Except(messageProvider.LastQueryDeliveryTypes).Count() > 0;

            return queueNotEmpty || newStorageMessages || doScheduledQuery || hasUnqueriedDeliveryTypes;
        }


        //цикл доставки сообщений
        internal virtual void StartDequeue()
        {
            //вычисляем доступное количество доставок по каждому каналу
            foreach (SendChannel<T> sender in _dispatcherContext.SenderProvider.GetSenders())
            {
                sender.AvailableLimitCapacity = sender.LimitManager.GetLimitCapacity();
            }
            
            //отправляем сообщения в очереди
            foreach (MessageProvider<T> messageProvider in _dispatcherContext.MessageProviders)
            {
                bool isMessageProviderReady = CanStartDequeueMessageProvider(messageProvider);
                if (!isMessageProviderReady)
                    continue;

                bool canContinue = DequeueMessageProvider(messageProvider);
                if (!canContinue)
                    break;
            }
            
            //вычисляем следующее время снятия лимитов
            foreach (SendChannel<T> sender in _dispatcherContext.SenderProvider.GetSenders())
            {
                sender.NextLimitsEndTimeUtc = sender.LimitManager.GetLimitsEndTimeUtc();
            }
        }

        protected virtual bool DequeueMessageProvider(MessageProvider<T> messageProvider)
        {
            bool canContinue = false;

            do
            {
                //отправить сообщения из очереди в памяти
                var dequeur = new ParallelDequeuer<T>(_dispatcherContext, messageProvider);
                canContinue = dequeur.DequeueAll();
                
                //получить новые сообщения в очередь из базы, если все из памяти отправлены
                //нужно знать, есть ли ещё не отправленные сообщения, даже если пока лимиты не позволяют отправить.
                if (messageProvider.MessageQueue.MemoryQueueLength == 0)
                {
                    QueryFromStorage(messageProvider);
                }
            }
            while (messageProvider.MessageQueue.MemoryQueueLength > 0 && canContinue);

            //если очередь пустая, то включаем мониторинг добавления сообщений в хранилище
            if (messageProvider.MessageQueue.MemoryQueueLength == 0
                && messageProvider.StorageInsertNotifier != null)
            {
                messageProvider.StorageInsertNotifier.StartMonitor();
            }
            
            return canContinue;
        }

        protected virtual void QueryFromStorage(MessageProvider<T> messageProvider)
        {
            //получить типы сообщений, которые могут быть отправлены
            messageProvider.LastQueryDeliveryTypes = _dispatcherContext.SenderProvider.GetActiveSenderTypes();
            
            //запросить из хранилища новые сообщения
            Stopwatch storageQueryTimer = Stopwatch.StartNew();

            messageProvider.MessageQueue.QueryStorage(messageProvider.LastQueryDeliveryTypes);            
            messageProvider.LastStorageQueryTimeUtc = DateTime.UtcNow;

            //отметить в сборщике статистики
            if (_dispatcherContext.StatisticsCollector != null)
            {
                _dispatcherContext.StatisticsCollector.StorageQueried(messageProvider, storageQueryTimer.Elapsed);
            }

            //отмечаем, что новые сообщения были получены
            if (messageProvider.StorageInsertNotifier != null)
                messageProvider.StorageInsertNotifier.HasUpdates = false;
        }
    }
}
