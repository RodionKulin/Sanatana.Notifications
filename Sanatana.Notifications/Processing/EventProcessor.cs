using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Microsoft.Extensions.Logging;
using Sanatana.Timers.Switchables;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.Models;

namespace Sanatana.Notifications.Processing
{
    public class EventProcessor<TKey> : ProcessorBase<TKey>, IRegularJob, IEventProcessor where TKey : struct
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
            DequeueAll();
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

                StartNextTask(() => ProcessSignal(item));
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

        protected void ProcessSignal(SignalWrapper<SignalEvent<TKey>> item)
        {
            try
            {
                if (item.Signal.EventSettingsId == null)
                {
                    SplitEvent(item);
                }
                else
                {
                    ComposeAndApplyResult(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                //increment fail counter and don't let same event to repeat exceptions multiple times 
                _eventQueue.ApplyResult(item, ProcessingResult.Fail);
            }
        }

        protected virtual void SplitEvent(SignalWrapper<SignalEvent<TKey>> item)
        {
            List<EventSettings<TKey>> eventSettings = _eventSettingsQueries
                .SelectByKey(item.Signal.EventKey).Result;

            if (eventSettings.Count == 0)
            {
                _eventQueue.ApplyResult(item, ProcessingResult.NoHandlerFound);
            }
            else if (eventSettings.Count == 1)
            {
                item.Signal.EventSettingsId = eventSettings.First().EventSettingsId;
                item.IsUpdated = true;

                ComposeAndApplyResult(item);
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

                _eventQueue.ApplyResult(item, ProcessingResult.Success);
                _eventQueue.Append(splitedEvents, false);
            }
        }

        protected virtual void ComposeAndApplyResult(SignalWrapper<SignalEvent<TKey>> item)
        {
            Stopwatch composeTimer = Stopwatch.StartNew();
            EventHandleResult<SignalDispatch<TKey>> composeResult = ComposeDispatches(item);
            composeTimer.Stop();

            _eventQueue.ApplyResult(item, composeResult.Result);
            _monitor.DispatchesComposed(item.Signal, composeTimer.Elapsed, composeResult.Result, composeResult.Items);
        }

        protected virtual EventHandleResult<SignalDispatch<TKey>> ComposeDispatches(SignalWrapper<SignalEvent<TKey>> @event)
        {
            //find matching EventHandler
            EventSettings<TKey> eventSettings = _eventSettingsQueries.Select(@event.Signal.EventSettingsId.Value).Result;
            if (eventSettings == null)
            {
                return EventHandleResult<SignalDispatch<TKey>>.FromResult(ProcessingResult.NoHandlerFound);
            }

            IEventHandler<TKey> eventHandler = _handlerRegistry.MatchHandler(eventSettings.EventHandlerId);
            if(eventHandler == null)
            {
                return EventHandleResult<SignalDispatch<TKey>>.FromResult(ProcessingResult.NoHandlerFound);
            }
            
            //compose dispatches for subscribers
            EventHandleResult<SignalDispatch<TKey>> composeResult = eventHandler.ProcessEvent(eventSettings, @event.Signal);
            if (composeResult.Result != ProcessingResult.Success)
            {
                return composeResult;
            }

            //enqueue dispatches
            _dispatchQueue.Append(composeResult.Items, false);

            @event.IsUpdated = true;
            if (composeResult.IsFinished == false)
            {
                composeResult.Result = ProcessingResult.Repeat;
            }

            return composeResult;
        }

    }
}
