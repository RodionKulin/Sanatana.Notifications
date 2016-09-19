using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Processors
{
    internal abstract class ProcessorBase<TKey>
        where TKey : struct
    {
        //поля
        protected SignaloBotHub<TKey> _context;
        protected Task[] _runningTasks;
        protected int _maxTasks;



        //инициализация
        public ProcessorBase(SignaloBotHub<TKey> context, int maxTasks)
        {
            _context = context;
            _maxTasks = maxTasks;
        }



        //методы
        public void DequeueAll()
        {
            while (CanContinue())
            {
                if(_runningTasks == null)
                {
                    _runningTasks = new Task[_maxTasks];
                }

                StartNextTask();   
            }

            WaitForCompletion();
            _runningTasks = null;
        }

        private void StartNextTask()
        {
            Task nextTask = null;
            int parallelTasks = _runningTasks.Count(p => p != null);

            if (parallelTasks < _runningTasks.Length)
            {
                nextTask = Task.Run(() => DequeueNext());
                _runningTasks[parallelTasks] = nextTask;
            }
            else
            {
                int finishedIndex = Task.WaitAny(_runningTasks);

                nextTask = _runningTasks[finishedIndex]
                    .ContinueWith(t => DequeueNext());

                _runningTasks[finishedIndex] = nextTask;
            }
            
            nextTask.ContinueWith(t =>
            {
                if (_context.Logger != null)
                    _context.Logger.Exception(t.Exception);
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        private void WaitForCompletion()
        {
            //в массиве не должно быть пустых ячеек перед запуском Task.WaitAll или Task.WaitAny
            Array.Resize(ref _runningTasks, _runningTasks.Count(p => p != null));

            //Task.WaitAll кидает AggregateException, если были необработанные исключения в списке тасков
            try
            {
                Task.WaitAll(_runningTasks);
            }
            catch (Exception exception)
            {
                if (_context.Logger != null)
                    _context.Logger.Exception(exception);
            }
        }

        protected abstract bool CanContinue();

        protected abstract Task DequeueNext();

    }
}
