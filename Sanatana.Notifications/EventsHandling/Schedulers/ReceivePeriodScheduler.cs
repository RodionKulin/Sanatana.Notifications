using Sanatana.Notifications.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.EventsHandling
{
    public class ReceivePeriodScheduler<TKey> : IScheduler<TKey>
        where TKey : struct
    {
        //properties
        protected ISubscriberScheduleSettingsQueries<TKey> _receivePeriodQueries;



        //init
        public ReceivePeriodScheduler(ISubscriberScheduleSettingsQueries<TKey> receivePeriodQueries)
        {
            _receivePeriodQueries = receivePeriodQueries;
        }


        //methods
        public virtual ProcessingResult SetSendingTime(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , List<Subscriber<TKey>> subscribers, List<SignalDispatch<TKey>> dispatches)
        {
            if (signalEvent.AddresseeType == AddresseeType.DirectAddresses)
            {
                return ProcessingResult.Success;
            }

            List<SubscriberScheduleSettings<TKey>> receivePeriods = SelectReceivePeriods(settings, subscribers);
           
            Dictionary<TKey, Subscriber<TKey>[]> subscribersBySubscriberId = subscribers
                .GroupBy(x => x.SubscriberId)
                .ToDictionary(x => x.Key, x => x.ToArray());
            Dictionary<TKey, SubscriberScheduleSettings<TKey>[]> receivePeriodsBySubscriberId = receivePeriods
                .GroupBy(x => x.SubscriberId)
                .ToDictionary(x => x.Key, x => x.ToArray());

            foreach (SignalDispatch<TKey> dispatch in dispatches)
            {
                if (dispatch.ReceiverSubscriberId == null
                    || dispatch.ScheduleSet == null
                    || !subscribersBySubscriberId.ContainsKey(dispatch.ReceiverSubscriberId.Value)
                    || !receivePeriodsBySubscriberId.ContainsKey(dispatch.ReceiverSubscriberId.Value))
                {
                    continue;
                }

                Subscriber<TKey> subscriber = subscribersBySubscriberId[dispatch.ReceiverSubscriberId.Value]
                    .FirstOrDefault(x => x.DeliveryType == dispatch.DeliveryType);
                if (subscriber == null)
                {
                    continue;
                }

                List<SubscriberScheduleSettings<TKey>> dispatchReceivePeriods = receivePeriodsBySubscriberId[dispatch.ReceiverSubscriberId.Value]
                    .Where(x => x.Set == dispatch.ScheduleSet.Value)
                    .ToList();

                bool isScheduled;
                dispatch.SendDateUtc = GetSendTimeUtc(subscriber.TimeZoneId, dispatchReceivePeriods, out isScheduled);
                dispatch.IsScheduled = isScheduled;
            }

            return ProcessingResult.Success;
        }

        protected virtual List<SubscriberScheduleSettings<TKey>> SelectReceivePeriods(
            EventSettings<TKey> settings, List<Subscriber<TKey>> subscribers)
        {
            List<int> receivePeriodsSetIds = settings.Templates
                .Where(x => x.ScheduleSet.HasValue)
                .Select(x => x.ScheduleSet.Value)
                .Distinct()
                .ToList();
            
            if (receivePeriodsSetIds.Count == 0)
            {
                return new List<SubscriberScheduleSettings<TKey>>();
            }

            List<TKey> subscriberIds = subscribers.Select(x => x.SubscriberId)
                .Distinct()
                .ToList();

            if (subscriberIds.Count == 0)
            {
                return new List<SubscriberScheduleSettings<TKey>>();
            }

            return _receivePeriodQueries.Select(subscriberIds, receivePeriodsSetIds).Result;
        }

        protected virtual DateTime GetSendTimeUtc(string timezoneId, List<SubscriberScheduleSettings<TKey>> periods, out bool isScheduled)
        {
            if (periods == null || periods.Count == 0)
            {
                isScheduled = false;
                return DateTime.UtcNow;
            }

            //check receive periods in subscriber timezone
            DateTime nowTime = GetNowInTimeZone(timezoneId);
            SubscriberScheduleSettings<TKey> nowPeriod = periods.FirstOrDefault(
                p => p.PeriodBegin <= nowTime.TimeOfDay
                && nowTime.TimeOfDay <= p.PeriodEnd);

            if (nowPeriod != null)
            {
                isScheduled = false;
                return DateTime.UtcNow;
            }

            DateTime sendDate = FindClosestSendDate(periods, nowTime);
            isScheduled = true;
            return sendDate.ToUniversalTime();
        }

        protected virtual DateTime GetNowInTimeZone(string timezoneId)
        {
            DateTime nowTime = DateTime.UtcNow;
            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(nowTime, timezone);
        }

        protected virtual DateTime FindClosestSendDate(List<SubscriberScheduleSettings<TKey>> periods, DateTime nowTime)
        {
            DateTime sendDate;
            List<SubscriberScheduleSettings<TKey>> todayPeriods = periods
                .Where(p => p.PeriodBegin >= nowTime.TimeOfDay)
                .ToList();
            List<SubscriberScheduleSettings<TKey>> tomorrowPeriods = periods
                .Where(p => p.PeriodBegin <= nowTime.TimeOfDay)
                .ToList();

            if (todayPeriods.Count > 0)
            {
                TimeSpan closestTime = todayPeriods.Select(p => p.PeriodBegin).Min();
                sendDate = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day,
                    closestTime.Hours, closestTime.Minutes, closestTime.Seconds);
            }
            else
            {
                TimeSpan closestTime = tomorrowPeriods.Select(p => p.PeriodBegin).Min();
                DateTime tomorrowDate = nowTime.AddDays(1);
                sendDate = new DateTime(tomorrowDate.Year, tomorrowDate.Month, tomorrowDate.Day,
                    closestTime.Hours, closestTime.Minutes, closestTime.Seconds);
            }

            return sendDate;
        }
    }
}
