using SignaloBot.Client.DelayScheduler;
using SignaloBot.DAL;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using SignaloBot.Client.Settings;
using SignaloBot.Client.Templates;
using SignaloBot.DAL.Entities.Results;
using SignaloBot.DAL.Entities.Parameters;

namespace SignaloBot.Client.Manager
{
    public class SignalManager
    {
        //свойства
        public SignaloBotContext Context { get; set; }


        //инициализация
        public SignalManager(SignaloBotContext context)
        {
            Context = context;
        }



        //составление сообщений
        public virtual void ConstructAndInsert(int deliveryType, int categoryID
            , bool setReceivePeriods, SubscriberParameters subscriberParameters
            , TemplateData bodyData, out Exception exception, TemplateData subjectData = null)
        {
            exception = null;

            List<Subscriber> subscribers = Context.Queries.Subscribers.Select(subscriberParameters, out exception);
            if (exception != null || subscribers.Count == 0)
            {
                return;
            }

            SignalTemplate template = Context.FindSignalTemplate(deliveryType, categoryID);
            List<Signal> messages = template.Build(subscribers, bodyData, subjectData);

            if (setReceivePeriods)
            {
                SetReceivePeriods(deliveryType, categoryID, messages, subscribers, out exception);
                if (exception != null)
                {
                    return;
                } 
            }
            
            Context.Queries.Signals.Insert(messages, out exception);
        }  

        public virtual void SetReceivePeriods(int deliveryType, int categoryID, List<Signal> messages
            , List<Subscriber> subscribers, out Exception exception)
        {
            List<UserReceivePeriod> periods = GetReceivePeriods(deliveryType, categoryID, messages, out exception);
            if (exception != null)
            {
                return;
            }

            IDelayScheduler delayScheduler = Context.FindDelayScheduler(deliveryType, categoryID);

            foreach (Signal message in messages)
            {
                Subscriber subscriber = subscribers.FirstOrDefault(p => p.UserID == message.ReceiverUserID);

                if (message.ReceiverUserID == null || subscriber == null)
                {
                    message.IsDelayed = false;
                    message.SendDateUtc = DateTime.UtcNow;
                }
                else
                {
                    List<UserReceivePeriod> userPeriods = periods.Where(
                        p => p.UserID == message.ReceiverUserID).ToList();

                    bool isDelayed;
                    DateTime sendDateUtc = delayScheduler.GetSendTime(subscriber.TimeZoneID
                        , userPeriods, out isDelayed);

                    message.IsDelayed = isDelayed;
                    message.SendDateUtc = sendDateUtc;
                }
            }
        }

        protected virtual List<UserReceivePeriod> GetReceivePeriods(int deliveryType, int categoryID 
            , List<Signal> messages, out Exception exception)
        {
            List<Guid> userIDs = messages.Where(p => p.ReceiverUserID != null)
                .Select(p => p.ReceiverUserID.Value).Distinct().ToList();

            if (userIDs.Count == 0)
            {
                exception = null;
                return new List<UserReceivePeriod>();
            }

            int periodsDeliveryType = Context.FindReceivePeriodDeliveryType(deliveryType, categoryID);
            int periodsCategoryID = Context.FindReceivePeriodCategoryID(deliveryType, categoryID);

            return Context.Queries.UserReceivePeriods.SelectCategory(userIDs,
                periodsDeliveryType, periodsCategoryID, out exception);
        }

        
        
        //изменение времени отправки отложенных сообщений
        public virtual void RescheduleDelayedMessages(Guid userID, string timezoneID
            , int deliveryType, int categoryID, out Exception exception)
        {
            int receiveDeliveryType = Context.FindReceivePeriodDeliveryType(deliveryType, categoryID);
            int receiveCategoryId = Context.FindReceivePeriodCategoryID(deliveryType, categoryID);

            List<UserReceivePeriod> receivePeriods = Context.Queries.UserReceivePeriods.SelectCategory(
                userID, receiveDeliveryType, receiveCategoryId, out exception);
            if (exception != null)
            {
                return;
            }

            List<Signal> delayedMessages = Context.Queries.Signals.SelectDelayed(
                userID, deliveryType, categoryID, out exception);
            if (exception != null)
            {
                return;
            }

            UpdateDelayedMessages(timezoneID, receivePeriods, delayedMessages, ref exception);
        }

        protected virtual void UpdateDelayedMessages(string timezoneID, List<UserReceivePeriod> receivePeriods
            , List<Signal> delayedMessages, ref Exception exception)
        {
            List<Signal> changedMessages = new List<Signal>();

            foreach (Signal message in delayedMessages)
            {
                IDelayScheduler scheduler = Context.FindDelayScheduler(message.DeliveryType, message.CategoryID);

                int receivePeriodCategoryID = Context.FindReceivePeriodCategoryID(
                    message.DeliveryType, message.CategoryID);

                List<UserReceivePeriod> categoryPeriods = receivePeriods.Where(
                    p => p.DeliveryType == message.DeliveryType
                    && p.CategoryID == receivePeriodCategoryID).ToList();                    

                bool isDelayed;
                DateTime sendDateUtc = scheduler.GetSendTime(timezoneID, categoryPeriods, out isDelayed);

                if (message.SendDateUtc != sendDateUtc)
                {
                    message.IsDelayed = isDelayed;
                    message.SendDateUtc = sendDateUtc;
                    changedMessages.Add(message);
                }
            }

            if (changedMessages.Count > 0)
            {
                Context.Queries.Signals.UpdateSendDateUtc(changedMessages, out exception);
            }
        }                
    }
}
