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
    internal class ParallelDequeuer<T>
        where T : IMessage
    {
        //поля
        protected MessageDispatcher<T> _dispatcherContext;
        protected MessageProvider<T> _messageProvider;
      

        //инициализация
        public ParallelDequeuer(MessageDispatcher<T> dispatcherContext, MessageProvider<T> messageProvider)
        {
            _dispatcherContext = dispatcherContext;
            _messageProvider = messageProvider;
        }


        //обработка очереди сообщений
        public bool DequeueAll()
        {
            bool canContinue = true;
            int tasksStarted = 0;
            int queueLength = _messageProvider.MessageQueue.MemoryQueueLength;

            int maxParallelTasks = _dispatcherContext.MaxParallelDeliveries < 1
                ? 1
                : _dispatcherContext.MaxParallelDeliveries;

            Task[] runningTasks = new Task[maxParallelTasks];

            while (tasksStarted < queueLength && canContinue)
            {
                StartNextTask(tasksStarted, runningTasks);
                tasksStarted++;

                //прервать при достижении лимитов
                canContinue = CanContinueDequeue();
            }

            WaitForCompletion(runningTasks);

            //прервать при достижении лимитов
            return CanContinueDequeue();
        }

        private void StartNextTask(int tasksStarted, Task[] runningTasks)
        {
            Task nextTask = null;
            int maxParallelTasks = runningTasks.Length;

            //запустить следующий поток
            if (tasksStarted < maxParallelTasks)
            {
                nextTask = Task.Run(() => DequeueNext());
                runningTasks[tasksStarted] = nextTask;
            }
            else
            {
                int finishedIndex = Task.WaitAny(runningTasks);
                nextTask = runningTasks[finishedIndex].ContinueWith(t => DequeueNext());
                runningTasks[finishedIndex] = nextTask;
            }

            //записать в лог необработанные ошибки в потоках
            nextTask.ContinueWith(t =>
            {
                if (_dispatcherContext.Logger != null)
                    _dispatcherContext.Logger.Exception(t.Exception);
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        private void WaitForCompletion(Task[] runningTasks)
        {
            //в массиве не должно быть пустых ячеек перед запуском Task.WaitAll или Task.WaitAny
            Array.Resize(ref runningTasks, runningTasks.Count(p => p != null));

            //Task.WaitAll кидает AggregateException, если были необработанные исключения в списке тасков
            try
            {
                Task.WaitAll(runningTasks.ToArray());
            }
            catch (Exception exception)
            {
                if (_dispatcherContext.Logger != null)
                    _dispatcherContext.Logger.Exception(exception);
            }
        }

        private bool CanContinueDequeue()
        {
            bool hasActiveSenders = _dispatcherContext.SenderProvider.GetSenders()
                .Any(p => p.IsActive && p.AvailableLimitCapacity > 0);

            return hasActiveSenders && _dispatcherContext.IsStarted;
        }


        //Отправление следующего сообщения
        private Task DequeueNext()
        {
            //получить следующее сообщение и отправителя
            T nextMessage = _messageProvider.MessageQueue.DequeueNext();           
            SendChannel<T> sendChannel = _dispatcherContext.SenderProvider.MatchSender(nextMessage);

            //результат
            SendResult sendResult;
            TimeSpan sendDuration;

            //отправить
            if (sendChannel.IsActive)
            {
                Stopwatch sendTimer = Stopwatch.StartNew();
                sendResult = sendChannel.Sender.Send(nextMessage);
                sendDuration = sendTimer.Elapsed;
            }
            else
            {
                _messageProvider.LastQueryDeliveryTypes.Remove(nextMessage.DeliveryType);
                sendResult = SendResult.Skipped;
                sendDuration = TimeSpan.FromSeconds(0);
            }

            //обработать результат
            HandleSendResult(sendChannel, nextMessage, sendResult, sendDuration);

            return Task.FromResult<int>(0);
        }

        private void HandleSendResult(SendChannel<T> sendChannel, T message, SendResult sendResult, TimeSpan sendDuration)
        {
            if (_dispatcherContext.StatisticsCollector != null)
            {
                _dispatcherContext.StatisticsCollector.MessageProcessed(sendChannel, message, sendResult, sendDuration);
            }

            if (sendResult == SendResult.Success)
            {
                sendChannel.MarkSuccessfulAttempt();
                _messageProvider.MessageQueue.ApplyResult(message, sendResult, SenderAvailability.NotChecked);
            }
            else if (sendResult == SendResult.Fail)
            {
                SenderAvailability senderAvailable = sendChannel.MarkFailedAttempt();
                _messageProvider.MessageQueue.ApplyResult(message, sendResult, senderAvailable);
            }
        }
    }
}
