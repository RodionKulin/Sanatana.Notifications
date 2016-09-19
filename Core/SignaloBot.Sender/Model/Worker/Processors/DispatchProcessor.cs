using Common.Utility;
using SignaloBot.DAL;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Processors
{
    internal class DispatchProcessor<TKey> : ProcessorBase<TKey>
        where TKey : struct
    {
        //поля
        private List<IDispatchQueue<TKey>> _dispatchQueues;



        //инициализация
        public DispatchProcessor(SignaloBotHub<TKey> context, List<IDispatchQueue<TKey>> dispatchQueues)
            : base(context, context.MaxParallelSenders)
        {
            _dispatchQueues = dispatchQueues;
        }



        //методы
        public bool OnTick()
        {
            if(!CanContinue(false))
            {
                return _context.State == SwitchState.Started;
            }
            
            //вычисляем доступное количество доставок по каждому каналу
            foreach (DispatchChannel<TKey> sender in _context.Senders)
            {
                sender.AvailableLimitCapacity = sender.LimitManager.GetLimitCapacity();
            }

            DequeueAll();

            //вычисляем следующее время снятия лимитов
            foreach (DispatchChannel<TKey> sender in _context.Senders)
            {
                sender.NextLimitsEndTimeUtc = sender.LimitManager.GetLimitsEndTimeUtc();
            }

            return _context.State == SwitchState.Started;
        }

        private bool CanContinue(bool checkLimitCapacity)
        {
            List<int> activeDeliveryTypes = _context.GetActiveSendersTypes(checkLimitCapacity);
            bool isEmpty = _dispatchQueues.All(p => p.CheckIsEmpty(activeDeliveryTypes));

            return !isEmpty && _context.State == SwitchState.Started;
        }

        protected override bool CanContinue()
        {
            List<int> activeDeliveryTypes = _context.GetActiveSendersTypes(true);
            bool isEmpty = _dispatchQueues.All(p => p.CheckIsEmpty(activeDeliveryTypes));

            return !isEmpty && _context.State == SwitchState.Started;
        }
               
        protected override Task DequeueNext()
        {
            //получить сообщение из очереди
            List<int> activeDeliveryTypes = _context.GetActiveSendersTypes(true);
            SignalWrapper<SignalDispatchBase<TKey>> item = null;
            IDispatchQueue<TKey> _dispatchQueue = null;

            foreach (IDispatchQueue<TKey> queue in _dispatchQueues)
            {
                _dispatchQueue = queue;
                item = queue.DequeueNext(activeDeliveryTypes);
                if (item != null)
                    break;
            }

            if(item == null)
            {
                return Task.FromResult(0);
            }
            
            //отправить
            DispatchChannel<TKey> sendChannel = _context.Senders.FirstOrDefault(
                p => p.IsActive
                && p.AvailableLimitCapacity > 0 
                && p.MatchChannel(item.Signal));

            ProcessingResult sendResult = ProcessingResult.Repeat;
            TimeSpan sendDuration = TimeSpan.FromSeconds(0);

            if (sendChannel != null)
            {
                Stopwatch sendTimer = Stopwatch.StartNew();
                sendResult = sendChannel.Sender.Send(item.Signal);
                sendDuration = sendTimer.Elapsed;
            }

            //применить результаты
            DispatcherAvailability senderAvailability = sendChannel.ApplyResult(sendResult);
            if(sendResult == ProcessingResult.Fail && senderAvailability == DispatcherAvailability.NotAvailable)
            {
                sendResult = ProcessingResult.Repeat;
            }

            _dispatchQueue.ApplyResult(item, sendResult);
            
            if (_context.StatisticsCollector != null)
            {
                _context.StatisticsCollector.DispatchSended(
                    item.Signal, sendDuration, sendResult, senderAvailability);
            }

            return Task.FromResult(0);
        }
        

    }
}
