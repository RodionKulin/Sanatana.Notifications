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

namespace Sanatana.Notifications.Composing
{
    public class SubscribersFetcher<TKey> : ISubscribersFetcher<TKey>
        where TKey : struct
    {
        //fields
        protected ISubscriberQueries<TKey> _subscriberQueries;


        //properties
        public int ItemsQueryLimit { get; set; }


        //init
        public SubscribersFetcher(SenderSettings senderSettings
            , ISubscriberQueries<TKey> subscriberQueries)
        {
            _subscriberQueries = subscriberQueries;

            ItemsQueryLimit = senderSettings.SubscribersFetcherItemsQueryLimit;
        }


        //method
        public virtual ComposeResult<Subscriber<TKey>> Select(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent)
        {
            if (signalEvent.AddresseeType == AddresseeType.DirectAddresses)
            {
                return ConvertAddressesToSubscribers(settings, signalEvent);
            }
            else if (signalEvent.AddresseeType == AddresseeType.SpecifiedSubscribersById)
            {
                var subscribersRange = new SubscribersRangeParameters<TKey>()
                {
                    FromSubscriberIds = signalEvent.PredefinedSubscriberIds
                };
                return QuerySubscribers(settings, signalEvent, subscribersRange);
            }
            else if (signalEvent.AddresseeType == AddresseeType.AllSubscsribers)
            {
                var subscribersRange = new SubscribersRangeParameters<TKey>()
                {
                    GroupId = signalEvent.GroupId,
                    SubscriberIdRangeFromIncludingSelf = signalEvent.SubscriberIdRangeFrom,
                    SubscriberIdRangeToIncludingSelf = signalEvent.SubscriberIdRangeTo,
                    SubscriberIdFromDeliveryTypesHandled = signalEvent.SubscriberIdFromDeliveryTypesHandled,
                    Limit = ItemsQueryLimit
                };
                return QuerySubscribers(settings, signalEvent, subscribersRange);
            }
            else
            {
                string message = string.Format(SenderInternalMessages.SubscribersFetcher_UnknownAddresseeType
                    , signalEvent.AddresseeType, signalEvent.GetType());
                throw new NotImplementedException(message);
            }
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
            EventSettings<TKey> settings, SignalEvent<TKey> signalEvent, SubscribersRangeParameters<TKey> rangeParameters)
        {
            if (settings.Subscription == null)
            {
                var exception = new NullReferenceException(string.Format(
                    SenderInternalMessages.Common_ParameterMissing, nameof(settings.Subscription)));
                return ComposeResult<Subscriber<TKey>>.FromResult(ProcessingResult.Fail);
            }

            rangeParameters.TopicId = signalEvent.TopicId ?? settings.Subscription.TopicId;

            bool categoryParameterChecked = settings.Subscription.CheckCategoryLastSendDate 
                || settings.Subscription.CheckCategoryEnabled 
                || settings.Subscription.CheckCategorySendCountNotGreater != null;
            rangeParameters.SelectFromCategories = settings.Subscription.CategoryId != null 
                && categoryParameterChecked;

            bool topicParameterChecked = settings.Subscription.CheckTopicLastSendDate 
                || settings.Subscription.CheckTopicEnabled 
                || settings.Subscription.CheckTopicSendCountNotGreater != null;
            rangeParameters.SelectFromTopics = settings.Subscription.CategoryId != null
                && rangeParameters.TopicId != null
                && topicParameterChecked;
            
            List<Subscriber<TKey>> subscribers = _subscriberQueries
                .Select(settings.Subscription, rangeParameters).Result;
            
            return new ComposeResult<Subscriber<TKey>>()
            {
                Items = subscribers,
                IsFinished = subscribers.Count < rangeParameters.Limit,
                Result = ProcessingResult.Success
            };
        }

    }
}
