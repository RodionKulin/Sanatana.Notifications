using Common.Utility;
using SignaloBot.Sender.Composers.Templates;
using SignaloBot.DAL;
using SignaloBot.Sender.Processors;
using SignaloBot.Sender.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers
{
    public abstract class ComposerBase<TKey>
        where TKey : struct
    {
        //свойства
        public virtual ICommonLogger Logger { get; set; }
        public virtual ISubscriberQueries<TKey> SubscriberQueries { get; set; }
        public virtual IUserReceivePeriodQueries<TKey> ReceivePeriodQueries { get; set; }
        public virtual IComposerQueries<TKey> ComposerQueries { get; set; }
        public virtual IDelayScheduler<TKey> DelayScheduler { get; set; }
        public virtual int SubscribersQueryCount { get; set; }


        //инициализация
        public ComposerBase()
        {
            SubscribersQueryCount = SenderConstants.COMPOSER_SUBSCRIBERS_QUERY_COUNT;
            DelayScheduler = new ReceivePeriodScheduler<TKey>();
        }



        //методы
        public virtual ComposeResult<SignalDispatchBase<TKey>> Compose(
            SignalEventBase<TKey> signalEvent, ComposerSettings<TKey> settings)
        {
            //select subscribers
            ComposeResult<Subscriber<TKey>> subscribers = SelectSubscribers(signalEvent, settings);
            if (subscribers.Result != ProcessingResult.Success)
            {
                return ComposeResult<SignalDispatchBase<TKey>>.FromResult(subscribers.Result);
            }
            if (subscribers.Items.Count > 0)
            {
                signalEvent.UserIDRangeFrom = subscribers.Items.Last().UserID;
            }
            List<IGrouping<int, Subscriber<TKey>>> deliveryGroups =
                subscribers.Items.GroupBy(p => p.DeliveryType).ToList();
            
            //select receive periods
            QueryResult<List<IGrouping<int, UserReceivePeriod<TKey>>>> receivePeriods =
                SelectReceivePeriods(deliveryGroups, settings);
            if (receivePeriods.HasExceptions)
            {
                return ComposeResult<SignalDispatchBase<TKey>>.FromResult(ProcessingResult.Repeat);
            }
            
            //build dispatches from templates
            List<SignalDispatchBase<TKey>> dispatches = new List<SignalDispatchBase<TKey>>();
            foreach (IGrouping<int, Subscriber<TKey>> groupSubscribers in deliveryGroups)
            {
                SignalTemplateBase<TKey> template = MatchTemplate(groupSubscribers.Key, settings);
                if (template == null)
                {
                    return ComposeResult<SignalDispatchBase<TKey>>.FromResult(ProcessingResult.NoHandlerFound);
                }

                List<Subscriber<TKey>> groupSubscribersList = groupSubscribers.ToList();
                List<SignalDispatchBase<TKey>> groupDispatches = BuildDispatches(
                    signalEvent, template, groupSubscribersList);
                
                if (template.ReceivePeriodsGroupID != null)
                {
                    List<UserReceivePeriod<TKey>> groupReceivePeriods = receivePeriods.Result
                        .FirstOrDefault(p => p.Key == template.ReceivePeriodsGroupID.Value)
                        ?.ToList()
                        ?? new List<UserReceivePeriod<TKey>>();

                    SetReceiveTime(groupDispatches, groupSubscribersList, groupReceivePeriods);
                }

                dispatches.AddRange(groupDispatches);
            }

            //return result
            return new ComposeResult<SignalDispatchBase<TKey>>()
            {
                Result = ProcessingResult.Success,
                Items = dispatches,
                IsFinished = subscribers.IsFinished
            };
        }

        protected virtual ComposeResult<Subscriber<TKey>> SelectSubscribers(
            SignalEventBase<TKey> signalEvent, ComposerSettings<TKey> settings)
        {
            bool? isFinished = null;
            var intermidiateResult = new SubscribersIntermidiateResult<TKey>();
            var range = new UsersRangeParameters<TKey>()
            {
                GroupID = signalEvent.GroupID,
                UserIDRangeFromExcludingSelf = signalEvent.UserIDRangeFrom,
                UserIDRangeToIncludingSelf = signalEvent.UserIDRangeTo,
                Limit = SubscribersQueryCount
            };

            if (settings.Subscribtion.SelectFromTopics)
            {
                QueryResult<List<UserTopicSettings<TKey>>> topicSubscribers =
                    SubscriberQueries.SelectTopics(settings.Subscribtion, range).Result;
                if (topicSubscribers.HasExceptions)
                {
                    return ComposeResult<Subscriber<TKey>>.FromResult(ProcessingResult.Repeat);
                }

                intermidiateResult.TopicSubscribers = topicSubscribers.Result;
                range.FromUserIDs = topicSubscribers.Result.Select(p => p.UserID).ToList();
                isFinished = isFinished ?? range.FromUserIDs.Count < range.Limit;
            }

            if (settings.Subscribtion.SelectFromCategories)
            {
                QueryResult<List<UserCategorySettings<TKey>>> categorySubscribers =
                    SubscriberQueries.SelectCategories(settings.Subscribtion, range).Result;
                if (categorySubscribers.HasExceptions)
                {
                    return ComposeResult<Subscriber<TKey>>.FromResult(ProcessingResult.Repeat);
                }

                intermidiateResult.CategorySubscribers = categorySubscribers.Result;
                range.FromUserIDs = categorySubscribers.Result.Select(p => p.UserID).Distinct().ToList();
                isFinished = isFinished ?? range.FromUserIDs.Count < range.Limit;
            }

            QueryResult<List<Subscriber<TKey>>> subscribers = SubscriberQueries
                .SelectDeliveryTypes(settings.Subscribtion, range, intermidiateResult).Result;
            if (subscribers.HasExceptions)
            {
                return ComposeResult<Subscriber<TKey>>.FromResult(ProcessingResult.Repeat);
            }

            return new ComposeResult<Subscriber<TKey>>()
            {
                Items = subscribers.Result,
                IsFinished = isFinished ?? subscribers.Result
                    .Select(p => p.UserID).Distinct().Count() < range.Limit,
                Result = ProcessingResult.Success
            };
        }

        protected virtual QueryResult<List<IGrouping<int, UserReceivePeriod<TKey>>>> SelectReceivePeriods(
            List<IGrouping<int, Subscriber<TKey>>> deliveryTypeUserGroups, ComposerSettings<TKey> settings)
        {
            List<int> deliveryTypes = deliveryTypeUserGroups
                .Select(p => p.Key)
                .ToList();

            List<SignalTemplateBase<TKey>> receivePeriodTemplates = settings.Templates
                .Where(p => p.ReceivePeriodsGroupID != null
                    && deliveryTypes.Contains(p.DeliveryType))
                .ToList();

            if (receivePeriodTemplates.Count == 0)
            {
                return new QueryResult<List<IGrouping<int, UserReceivePeriod<TKey>>>>(null, false);
            }

            List<int> templateDeliveryTypes = receivePeriodTemplates
                .Select(p => p.DeliveryType)
                .Distinct()
                .ToList();

            List<int> receivePeriodsGroups = receivePeriodTemplates
                .Select(p => (int)p.ReceivePeriodsGroupID)
                .Distinct()
                .ToList();

            List<TKey> userIDs = deliveryTypeUserGroups
                .Where(p => templateDeliveryTypes.Contains(p.Key))
                .SelectMany(p => p)
                .Select(p => p.UserID)
                .Distinct()
                .ToList();

            if (userIDs.Count == 0)
            {
                return new QueryResult<List<IGrouping<int, UserReceivePeriod<TKey>>>>(null, false);
            }

            QueryResult<List<UserReceivePeriod<TKey>>> receivePeriods =
                ReceivePeriodQueries.Select(userIDs, receivePeriodsGroups).Result;

            if (receivePeriods.HasExceptions)
            {
                return new QueryResult<List<IGrouping<int, UserReceivePeriod<TKey>>>>(null, true);
            }

            List<IGrouping<int, UserReceivePeriod<TKey>>> receivePeriodGroups =
                receivePeriods.Result.GroupBy(p => p.ReceivePeriodsGroupID).ToList();

            return new QueryResult<List<IGrouping<int, UserReceivePeriod<TKey>>>>(
                receivePeriodGroups, false);
        }

        protected virtual SignalTemplateBase<TKey> MatchTemplate(
            int deliveryType, ComposerSettings<TKey> settings)
        {
            SignalTemplateBase<TKey> template = settings.Templates
                .FirstOrDefault(p => p.DeliveryType == deliveryType);

            if (template == null && Logger != null)
            {
                Logger.Error(InternalMessages.NoTemplateMatched, deliveryType);
            }

            return template;
        }

        protected abstract List<SignalDispatchBase<TKey>> BuildDispatches(
            SignalEventBase<TKey> signalEvent, SignalTemplateBase<TKey> template
            , List<Subscriber<TKey>> subscribers);
        
        protected virtual void SetReceiveTime(List<SignalDispatchBase<TKey>> dispatches
            , List<Subscriber<TKey>> subscribers, List<UserReceivePeriod<TKey>> receivePeriods)
        {
            foreach (SignalDispatchBase<TKey> dispatch in dispatches)
            {
                if (dispatch.ReceiverUserID == null)
                {
                    continue;
                }

                Subscriber<TKey> subscriber = subscribers.FirstOrDefault(
                    p => EqualityComparer<TKey>.Default.Equals(p.UserID, dispatch.ReceiverUserID.Value));

                if(subscriber == null)
                {
                    continue;
                }
                
                List<UserReceivePeriod<TKey>> subscriberReceivePeriods = receivePeriods.Where(
                    p => EqualityComparer<TKey>.Default.Equals(p.UserID, subscriber.UserID))
                    .ToList();

                bool isDelayed;
                DateTime sendDateUtc = DelayScheduler.GetSendTime(subscriber.TimeZoneID
                    , receivePeriods, out isDelayed);

                dispatch.IsDelayed = isDelayed;
                dispatch.SendDateUtc = sendDateUtc;
            }
        }
        


        //IDisposable
        public virtual void Dispose()
        {
        }
    }
}
