using SignaloBot.DAL;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.DelayScheduler
{
    public class ReceivePeriodScheduler : IDelayScheduler
    {     

        //методы
        public virtual DateTime GetSendTime(string timezoneID, List<UserReceivePeriod> periods, out bool isDelayed)
        {
            if (periods == null || periods.Count == 0)
            {
                isDelayed = false;
                return DateTime.UtcNow;
            }

            //получаем текущее время в заданном часовом поясе
            DateTime nowTime = GetNowInTimeZone(timezoneID);

            
            //ищем соответсвующий период времени
            UserReceivePeriod nowPeriod = periods.FirstOrDefault(
                p => p.PeriodBegin <= nowTime.TimeOfDay
                && nowTime.TimeOfDay <= p.PeriodEnd);
            

            //сразу отправляем, если период найден
            if (nowPeriod != null)
            {
                isDelayed = false;
                return DateTime.UtcNow;
            }
            //откладываем, если время вне разрешённых периодов
            else
            {
                DateTime sendDate = FindClosestSendDate(periods, nowTime);

                isDelayed = true;
                return sendDate.ToUniversalTime();
            }
        }

        protected virtual DateTime GetNowInTimeZone(string timezoneID)
        {
            DateTime nowTime = DateTime.UtcNow;

            TimeZoneInfo timezone;
            try
            {
                timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneID);
            }
            catch
            {
                bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(nowTime);
                TimeSpan timeOffset = isDaylight 
                    ? TimeSpan.FromHours(3)
                    : TimeSpan.FromHours(4);

                timezone = TimeZoneInfo.CreateCustomTimeZone("Moscow TimeZone", timeOffset,
                    "Russian Standard Time", "Russian Standard Time");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
        }

        protected virtual DateTime FindClosestSendDate(List<UserReceivePeriod> periods, DateTime nowTime)
        {
            List<UserReceivePeriod> todayPeriods = periods.Where(p => p.PeriodBegin >= nowTime.TimeOfDay).ToList();
            List<UserReceivePeriod> tomorrowPeriods = periods.Where(p => p.PeriodBegin <= nowTime.TimeOfDay).ToList();
            DateTime sendDate;

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
