using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.DispatchHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Microsoft.Extensions.Logging;
using Sanatana.Timers.Switchables;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.Processing.Interfaces;

namespace Sanatana.Notifications.Processing
{
    public class EventProcessor<TKey> : ProcessorBase<TKey>, IRegularJob, ICompositionProcessor where TKey : struct
    {
        //fields
        protected SenderState<TKey> _hubState;
        protected IMonitor<TKey> _monitor;
        protected IEventQueue<TKey> _eventQueue;
        protected IDispatchQueue<TKey> _dispatchQueue;
        protected IEventHandlerRegistry<TKey> _handlerRegistry;
        protected IEventSettingsQueries<TKey> _eventSettingsQueries;


        //init
        public EventProcessor(SenderState<TKey> hubState, IMonitor<TKey> eventSink
            , ILogger logger, SenderSettings senderSettings
            , IEventQueue<TKey> eventQueue, IDispatchQueue<TKey> dispatchQueue
            , IEventHandlerRegistry<TKey> handlerRegistry, IEventSettingsQueries<TKey> eventSettingsQueries)
            : base(logger)
        {
            _hubState = hubState;
            _monitor = eventSink;
            _eventQueue = eventQueue;
            _dispatchQueue = dispatchQueue;
            _handlerRegistry = handlerRegistry;
            _eventSettingsQueries = eventSettingsQueries;

            MaxParallelItems = senderSettings.MaxParallelEventsProcessed;
        }


        //IRegularJob methods
        public virtual void Tick()
        {
            if (CanContinue())
            {
                DequeueAll();
            }

            return;
        }
        public virtual void Flush()
        {
        }




        //processing methods
        protected virtual void DequeueAll()
        {
            while (CanContinue())
            {
                SignalWrapper<SignalEvent<TKey>> item = _eventQueue.DequeueNext();
                if (item == null)
                {
                    break;
                }

                StartNextTask(() => ProcessSignal(item, _eventQueue));
            }

            WaitForCompletion();
        }

        protected virtual bool CanContinue()
        {
            bool isQueueEmpty = _eventQueue.CountQueueItems() == 0;

            int actualDispatches = _dispatchQueue.CountQueueItems();
            int maxDispatches = _dispatchQueue.PersistBeginOnItemsCount;
            bool isDispatchQueueFull = actualDispatches >= maxDispatches;

            return !isQueueEmpty && !isDispatchQueueFull && _hubState.State == SwitchState.Started;
        }

        protected void ProcessSignal(SignalWrapper<SignalEvent<TKey>> item, IEventQueue<TKey> eventQueue)
        {
            try
            {
                if (item.Signal.EventSettingsId == null)
                {
                    SplitEvent(item, eventQueue);
                }
                else
                {
                    ComposeAndApplyResult(item, eventQueue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                //increment fail counter and don't let same event to repeat exceptions multiple times 
                eventQueue.ApplyResult(item, ProcessingResult.Fail);
            }
        }

        protected virtual void SplitEvent(SignalWrapper<SignalEvent<TKey>> item, IEventQueue<TKey> eventQueue)
        {
            List<EventSettings<TKey>> eventSettings = _eventSettingsQueries
                .Select(item.Signal.CategoryId).Result;

            if (eventSettings.Count == 0)
            {
                eventQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
            }
            else if (eventSettings.Count == 1)
            {
                item.Signal.EventSettingsId = eventSettings.First().EventSettingsId;
                item.IsUpdated = true;

                ComposeAndApplyResult(item, eventQueue);
            }
            else if (eventSettings.Count > 1)
            {
                var splitedEvents = new List<SignalEvent<TKey>>();
                foreach (EventSettings<TKey> settings in eventSettings)
                {
                    SignalEvent<TKey> clone = item.Signal.CreateClone();
                    clone.SignalEventId = default(TKey);
                    clone.EventSettingsId = settings.EventSettingsId;
                    splitedEvents.Add(clone);
                }

                eventQueue.ApplyResult(item, ProcessingResult.Success);
                eventQueue.Append(splitedEvents, false);
            }
        }

        protected virtual void ComposeAndApplyResult(SignalWrapper<SignalEvent<TKey>> item, IEventQueue<TKey> eventQueue)
        {
            Stopwatch composeTimer = Stopwatch.StartNew();

            EventHandleResult<SignalDispatch<TKey>> composeResult = ComposeDispatches(item);
            eventQueue.ApplyResult(item, composeResult.Result);

            TimeSpan composeDuration = composeTimer.Elapsed;
            _monitor.DispatchesComposed(
                item.Signal, composeDuration, composeResult.Result, composeResult.Items);
        }

        protected virtual EventHandleResult<SignalDispatch<TKey>> ComposeDispatches(SignalWrapper<SignalEvent<TKey>> item)
        {
            EventSettings<TKey> eventSettings = _eventSettingsQueries
                .Select(item.Signal.EventSettingsId.Value).Result;
            if (eventSettings == null)
            {
                return EventHandleResult<SignalDispatch<TKey>>.FromResult(ProcessingResult.NoHandlerFound);
            }

            IEventHandler<TKey> compositionHandler =
                _handlerRegistry.MatchHandler(eventSettings.CompositionHandlerId);
            if(compositionHandler == null)
            {
                return EventHandleResult<SignalDispatch<TKey>>.FromResult(ProcessingResult.NoHandlerFound);
            }

            EventHandleResult<SignalDispatch<TKey>> composeResult = 
                compositionHandler.ProcessEvent(eventSettings, item.Signal);
            if (composeResult.Result == ProcessingResult.Success)
            {
                item.IsUpdated = true;

                _dispatchQueue.Append(composeResult.Items, false);

                if (composeResult.IsFinished == false)
                {
                    composeResult.Result = ProcessingResult.Repeat;
                }
            }

            return composeResult;
        }

    }
}
