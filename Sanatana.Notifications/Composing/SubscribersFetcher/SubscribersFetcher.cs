using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Resources;
using Sanatana.Notifications.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Extensions;

namespace Sanatana.Notifications.Composing
{
    public class SubscribersFetcher<TKey> : ISubscribersFetcher<TKey>
        where TKey : struct
    {
        //fields
        protected ISubscriberQueries<TKey> _subscriberQueries;
        protected ILogger _logger;


        //properties
        public int ItemsQueryLimit { get; set; }


        //init
        public SubscribersFetcher(ILogger logger, SenderSettings senderSettings
            , ISubscriberQueries<TKey> subscriberQueries)
        {
            _logger = logger;
            _subscriberQueries = subscriberQueries;

            ItemsQueryLimit = senderSettings.SubscribersFetcherItemsQueryLimit;
        }


        //method
        public virtual ComposeResult<Subscriber<TKey>> Select(EventSettings<TKey> eventSettings, SignalEvent<TKey> signalEvent)
        {
            if (signalEvent.AddresseeType == AddresseeType.DirectAddresses)
            {
                return ConvertAddressesToSubscribers(eventSettings, signalEvent);
            }
            
            if (signalEvent.AddresseeType == AddresseeType.SubscriberIds)
            {
                var subscribersRange = new SubscribersRangeParameters<TKey>()
                {
                    FromSubscriberIds = signalEvent.PredefinedSubscriberIds
                };
                return QuerySubscribers(eventSettings, signalEvent, subscribersRange);
            }
            
            if (signalEvent.AddresseeType == AddresseeType.SubscriptionParameters)
            {
                var subscribersRange = new SubscribersRangeParameters<TKey>()
                {
                    SubscriberIdRangeFromIncludingSelf = signalEvent.SubscriberIdRangeFrom,
                    SubscriberIdRangeToIncludingSelf = signalEvent.SubscriberIdRangeTo,
                    SubscriberIdFromDeliveryTypesHandled = signalEvent.SubscriberIdFromDeliveryTypesHandled,
                    Limit = ItemsQueryLimit
                };
                return QuerySubscribers(eventSettings, signalEvent, subscribersRange);
            }

            string message = string.Format(SenderInternalMessages.SubscribersFetcher_UnknownAddresseeType
                , signalEvent.AddresseeType, signalEvent.GetType());
            throw new NotImplementedException(message);
        }

        protected virtual ComposeResult<Subscriber<TKey>> ConvertAddressesToSubscribers(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent)
        {
            List<Subscriber<TKey>> subscribers = signalEvent.PredefinedAddresses.Select(x => new Subscriber<TKey>
            {
                DeliveryType = x.DeliveryType,
                Address = x.Address,
                Language = x.Language
            })
            .ToList();

            return new ComposeResult<Subscriber<TKey>>
            {
                Items = subscribers,
                IsFinished = true,
                Result = ProcessingResult.Success
            };
        }

        protected virtual ComposeResult<Subscriber<TKey>> QuerySubscribers(
            EventSettings<TKey> eventSettings, SignalEvent<TKey> signalEvent, SubscribersRangeParameters<TKey> rangeParameters)
        {
            if (eventSettings.Subscription == null)
            {
                _logger.LogError(SenderInternalMessages.Common_ParameterMissing, nameof(eventSettings.Subscription));
                return ComposeResult<Subscriber<TKey>>.FromResult(ProcessingResult.Fail);
            }

            //use TopicId from SignalEvent over default one in EventSettings
            //if both are null will not query for topic
            rangeParameters.TopicId = signalEvent.TopicId ?? eventSettings.Subscription.TopicId;

            //overwrite Subscription filters data with SignalEvent filters data
            rangeParameters.SubscriberFilters = new Dictionary<string, string>();
            rangeParameters.SubscriberFilters.Merge(eventSettings.Subscription.SubscriberFiltersData);
            rangeParameters.SubscriberFilters.Merge(signalEvent.SubscriberFiltersData);
            
            bool categoryParameterChecked = eventSettings.Subscription.CheckCategoryLastSendDate 
                || eventSettings.Subscription.CheckCategoryEnabled 
                || eventSettings.Subscription.CheckCategorySendCountNotGreater != null;
            rangeParameters.SelectFromCategories = eventSettings.Subscription.CategoryId != null 
                && categoryParameterChecked;

            bool topicParameterChecked = eventSettings.Subscription.CheckTopicLastSendDate 
                || eventSettings.Subscription.CheckTopicEnabled 
                || eventSettings.Subscription.CheckTopicSendCountNotGreater != null;
            //if delivery type is not specified, will look for topic settings for all delivery types
            rangeParameters.SelectFromTopics = eventSettings.Subscription.CategoryId != null
                && rangeParameters.TopicId != null
                && topicParameterChecked;
            
            List<Subscriber<TKey>> subscribers = _subscriberQueries
                .Select(eventSettings.Subscription, rangeParameters).Result;
            
            return new ComposeResult<Subscriber<TKey>>()
            {
                Items = subscribers,
                IsFinished = subscribers.Count < rangeParameters.Limit,
                Result = ProcessingResult.Success
            };
        }

    }
}
