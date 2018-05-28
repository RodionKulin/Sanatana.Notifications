using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Queues;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.Composing
{
    public class CompositionHandler<TKey> : ICompositionHandler<TKey>
        where TKey : struct
    {
        //fields
        protected ISubscribersFetcher<TKey> _subscribersFetcher;
        protected IDispatchBuilder<TKey> _dispatchBuilder;
        protected IScheduler<TKey> _scheduler;
        protected ISubscriberQueries<TKey> _subscriberQueries;

        //properties
        public int? CompositionHandlerId { get; set; }


        //init
        public CompositionHandler(ISubscribersFetcher<TKey> subscribersFetcher
            , IDispatchBuilder<TKey> dispatchBuilder, IScheduler<TKey> scheduler
            , ISubscriberQueries<TKey> subscriberQueries)
        {
            _subscribersFetcher = subscribersFetcher;
            _dispatchBuilder = dispatchBuilder;
            _scheduler = scheduler;
            _subscriberQueries = subscriberQueries;
        }


        //methods
        public virtual ComposeResult<SignalDispatch<TKey>> ProcessEvent(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent)
        {
            //check is any active templates exist
            bool hasActiveTemplates = settings.Templates.Any(x => x.IsActive);
            if(hasActiveTemplates == false)
            {
                return new ComposeResult<SignalDispatch<TKey>>()
                {
                    Result = ProcessingResult.Success,
                    Items = new List<SignalDispatch<TKey>>(),
                    IsFinished = true
                };
            }
            
            //select subscribers
            ComposeResult<Subscriber<TKey>> subscribers = FetchSubscribers(settings, signalEvent);
            if (subscribers.Result != ProcessingResult.Success)
            {
                return ComposeResult<SignalDispatch<TKey>>.FromResult(subscribers.Result);
            }

            //build dispatches
            ComposeResult<SignalDispatch<TKey>> dispatches = BuildDispatches(
                settings, signalEvent, subscribers.Items);
            if (dispatches.Result != ProcessingResult.Success)
            {
                return dispatches;
            }

            //schedule send time
            ProcessingResult scheduleResult = ScheduleDispatches(settings, signalEvent, subscribers.Items, dispatches.Items);
            if (scheduleResult != ProcessingResult.Success)
            {
                return ComposeResult<SignalDispatch<TKey>>.FromResult(scheduleResult);
            }

            //update subscribers
            ProcessingResult updateResult = UpdateSubscribers(settings, signalEvent, subscribers.Items, dispatches.Items);
            if (updateResult != ProcessingResult.Success)
            {
                return ComposeResult<SignalDispatch<TKey>>.FromResult(updateResult);
            }

            //remember latest subscriber handled to resume on next iteration
            SetCurrentProgress(signalEvent, subscribers.Items);

            //result
            return new ComposeResult<SignalDispatch<TKey>>()
            {
                Result = ProcessingResult.Success,
                Items = dispatches.Items,
                IsFinished = subscribers.IsFinished
            };
        }

        protected virtual ComposeResult<Subscriber<TKey>> FetchSubscribers(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent)
        {
            ComposeResult<Subscriber<TKey>> subscribers = _subscribersFetcher.Select(settings, signalEvent);
            return subscribers;
        }

        protected virtual ComposeResult<SignalDispatch<TKey>> BuildDispatches(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            ComposeResult<SignalDispatch<TKey>> dispatches = _dispatchBuilder.Build(
                 settings, signalEvent, subscribers);
            return dispatches;
        }

        protected virtual ProcessingResult ScheduleDispatches(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , List<Subscriber<TKey>> subscribers, List<SignalDispatch<TKey>> dispatches)
        {
            ProcessingResult scheduleResult = _scheduler.SetSendingTime(settings, signalEvent, subscribers, dispatches);
            return scheduleResult;
        }

        protected virtual ProcessingResult UpdateSubscribers(
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , List<Subscriber<TKey>> subscribers, List<SignalDispatch<TKey>> dispatches)
        {
            if (settings.Updates == null
                || signalEvent.AddresseeType == AddresseeType.DirectAddresses)
            {
                return ProcessingResult.Success;
            }

            _subscriberQueries.Update(settings.Updates, dispatches);
            return ProcessingResult.Success;
        }

        protected virtual void SetCurrentProgress(SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            if (subscribers.Count > 0 
                && signalEvent.AddresseeType == AddresseeType.AllSubscsribers)
            {
                subscribers = subscribers.OrderByDescending(x => x.SubscriberId).ToList();    //assume that SubscriberIds are ordered in storage
                TKey latestSubscriberId = subscribers.First().SubscriberId;
                signalEvent.SubscriberIdRangeFrom = latestSubscriberId;
                signalEvent.SubscriberIdFromDeliveryTypesHandled = subscribers
                    .TakeWhile(x => EqualityComparer<TKey>.Default.Equals(x.SubscriberId, latestSubscriberId))
                    .Select(x => x.DeliveryType)
                    .ToList();
            }
        }
    }
}
