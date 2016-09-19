using Common.Utility;
using SignaloBot.DAL;
using SignaloBot.Sender.Composers;
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
    internal class EventProcessor<TKey> : ProcessorBase<TKey>
        where TKey : struct
    {
        //поля
        private List<IEventQueue<TKey>> _eventQueues;



        //инициализация
        public EventProcessor(SignaloBotHub<TKey> context, List<IEventQueue<TKey>> eventQueues)
            : base(context, context.MaxParallelComposers)
        {
            _eventQueues = eventQueues;
        }



        //методы
        public bool OnTick()
        {
            if (CanContinue())
            {
                DequeueAll();
            }

            return _context.State == SwitchState.Started;
        }

        protected override bool CanContinue()
        {
            if(_context.Composer == null)
            {
                return false;
            }
            
            bool isEmpty = _eventQueues.All(p => p.CountQueueItems() == 0);
            
            int actualItems = _context.DispatchQueues.Sum(p => p.CountQueueItems());
            int maxItems = _context.DispatchQueues.Sum(p => p.ReturnToStorageAfterItemsCount);
            bool dispatchQueuesFull = actualItems >= maxItems;

            return !isEmpty && !dispatchQueuesFull && _context.State == SwitchState.Started;
        }

        protected override Task DequeueNext()
        {
            //dequeue       
            SignalWrapper<SignalEventBase<TKey>> item = null;
            IEventQueue<TKey> eventQueue = null;
            
            foreach (IEventQueue<TKey> queue in _eventQueues)
            {
                eventQueue = queue;
                item = queue.DequeueNext();
                if (item != null)
                    break;
            }
            
            //process
            if (item == null)
            {
                return Task.FromResult(0);
            }

            if (item.Signal.ComposerSettingsID == null)
            {
                SplitEvent(item, eventQueue);
            }
            else
            {
                ComposeDispatches(item, eventQueue);
            }
            
            return Task.FromResult(0);
        }

        private void SplitEvent(
            SignalWrapper<SignalEventBase<TKey>> item, IEventQueue<TKey> eventQueue)
        {
            QueryResult<List<ComposerSettings<TKey>>> composerSettings = _context.Composer.ComposerQueries
                .Select(item.Signal.CategoryID).Result;
            
            if(composerSettings.HasExceptions)
            {
                eventQueue.ApplyResult(item, ProcessingResult.Repeat);
            }
            else if (composerSettings.Result.Count == 0)
            {
                eventQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
            }
            else if(composerSettings.Result.Count == 1)
            {
                item.Signal.ComposerSettingsID = composerSettings.Result.First().ComposerSettingsID;
                item.IsUpdated = true;

                ComposeDispatches(item, eventQueue);
            }
            else if (composerSettings.Result.Count > 1)
            {
                var splitEvents = new List<SignalEventBase<TKey>>();
                foreach (ComposerSettings<TKey> settings in composerSettings.Result)
                {
                    SignalEventBase<TKey> clone = item.Signal.CreateClone();
                    clone.SignalEventID = default(TKey);
                    clone.IsSplitted = true;
                    clone.ComposerSettingsID = settings.ComposerSettingsID;
                    splitEvents.Add(clone);
                }

                eventQueue.ApplyResult(item, ProcessingResult.Success);
                eventQueue.Append(splitEvents, false);
            }
        }

        private void ComposeDispatches(
            SignalWrapper<SignalEventBase<TKey>> item, IEventQueue<TKey> eventQueue)
        {
            //получить композитора
            QueryResult<ComposerSettings<TKey>> composerSettings = _context.Composer.ComposerQueries
                .Select(item.Signal.ComposerSettingsID.Value).Result;
            
            if (composerSettings.HasExceptions)
            {
                eventQueue.ApplyResult(item, ProcessingResult.Repeat);
                return;
            }
            else if (composerSettings.Result == null)
            {
                eventQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
                return;
            }

            //составить
            ComposeResult<SignalDispatchBase<TKey>> composeResult;
            Stopwatch composeTimer = Stopwatch.StartNew();

            do
            {
                composeResult = _context.Composer.Compose(item.Signal, composerSettings.Result);
                item.IsUpdated = true;

                if (composeResult.Result == ProcessingResult.Success)
                {
                    IDispatchQueue<TKey> dispatchQueue = _context.DispatchQueues.FirstOrDefault();
                    dispatchQueue.Append(composeResult.Items, false);
                }
            }
            while (!composeResult.IsFinished
                && composeResult.Result == ProcessingResult.Success
                && CanContinue());

            //применить результаты
            TimeSpan composeDuration = composeTimer.Elapsed;
            if (composeResult.Result == ProcessingResult.Success && !composeResult.IsFinished)
            {
                composeResult.Result = ProcessingResult.Repeat;
            }
            eventQueue.ApplyResult(item, composeResult.Result);
            
            if (_context.StatisticsCollector != null)
            {
                _context.StatisticsCollector.DispatchesComposed(
                    item.Signal, composeDuration, composeResult.Result);
            }
        }
    }
}
