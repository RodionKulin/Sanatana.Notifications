using Microsoft.Extensions.Logging;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Processing
{
    public abstract class ProcessorBase<TKey>
        where TKey : struct
    {
        //fields
        protected ILogger _logger;
        protected int _maxParallelItems;
        protected Task[] _runningTasks;


        //properties
        /// <summary>
        /// Number of parallel items processed. By default equal to processors count.
        /// </summary>
        public int MaxParallelItems
        {
            get
            {
                return _maxParallelItems;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(SenderInternalMessages.ProcessorBase_ParallelNumberOutOfRange);
                }

                _maxParallelItems = value;
            }
        }


        //init
        public ProcessorBase(ILogger logger)
        {
            _logger = logger;
            MaxParallelItems = Environment.ProcessorCount;
        }



        //methods
        protected virtual void StartNextTask(Action action)
        {
            if (_runningTasks == null)
            {
                _runningTasks = new Task[_maxParallelItems];
            }

            Task nextTask = null;
            int runningTasksCount = _runningTasks.Count(p => p != null);

            if (runningTasksCount < _runningTasks.Length)
            {
                nextTask = Task.Run(action);
                _runningTasks[runningTasksCount] = nextTask;
            }
            else
            {
                int finishedIndex = Task.WaitAny(_runningTasks);
                nextTask = _runningTasks[finishedIndex].ContinueWith(t => action());
                _runningTasks[finishedIndex] = nextTask;
            }
            
            nextTask.ContinueWith(t =>
            {
                _logger.LogError(t.Exception, null);
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        protected virtual void WaitForCompletion()
        {
            if(_runningTasks == null)
            {
                return;
            }

            //Array should not have any empty cells before calling Task.WaitAll or Task.WaitAny
            int runningTasksCount = _runningTasks.Count(p => p != null);
            Array.Resize(ref _runningTasks, runningTasksCount);

            Task.WaitAll(_runningTasks);

            _runningTasks = null;
        }


    }
}
