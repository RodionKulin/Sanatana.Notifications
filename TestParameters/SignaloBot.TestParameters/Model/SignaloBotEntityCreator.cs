using SignaloBot.DAL;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using SignaloBot.TestParameters.Model;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SignaloBot.Client.Manager;

namespace SignaloBot.TestParameters.Model
{
    public class SignaloBotEntityCreator
    {
        //методы
        public static UserDeliveryTypeSettings CreateUserDeliveryTypeSettings(Guid userId)
        {
            return new UserDeliveryTypeSettings()
            {
                UserID = userId,
                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                Address = "fake@aa.aa",
               
                TimeZoneID = null,
                LastUserVisitUtc = null,
                LastSendDateUtc = null,

                IsEnabled = true,
                IsEnabledOnNewTopic = true,

                SendCount = 0,

                NDRCount = 0,
                IsBlockedOfNDR = false,
                BlockOfNDRResetCode = null,
                BlockOfNDRResetCodeSendDateUtc = null
            };
        }

        public static UserCategorySettings CreateUserCategorySettings(int? categoryID = null)
        {
            categoryID = categoryID ?? SignaloBotTestParameters.ExistingCategoryID;

            return new UserCategorySettings()
            {
                UserID = SignaloBotTestParameters.ExistingUserID,
                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                CategoryID = categoryID.Value,
                LastSendDateUtc = DateTime.UtcNow,
                IsEnabled = true,
                SendCount = 0
            };
        }

        public static UserTopicSettings CreateUserTopicSettings(int? categoryID = null, int? topicID = null)
        {
            categoryID = categoryID ?? SignaloBotTestParameters.ExistingCategoryID;
            topicID = topicID ?? SignaloBotTestParameters.ExistingSubscriptionTopicID;

            return new UserTopicSettings()
            {
                UserID = SignaloBotTestParameters.ExistingUserID,
                CategoryID = categoryID.Value,
                TopicID = topicID.Value,
                IsDeleted = false,
                IsEnabled = true,
                AddDateUtc = DateTime.UtcNow
            };
        }

        public static UserReceivePeriod CreateUserReceivePeriod(int deliveryType, int order)
        {
            return new UserReceivePeriod()
            {
                UserID = SignaloBotTestParameters.ExistingUserID,
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                DeliveryType = deliveryType,
                PeriodOrder = order,
                PeriodBegin = TimeSpan.FromHours(0),
                PeriodEnd = TimeSpan.FromHours(24)
            };
        }

        public static Signal CreateSignal()
        {
            return new Signal()
            {
                //SignalID = устанавливается в базе

                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                TopicID = SignaloBotTestParameters.ExistingSubscriptionTopicID,

                ReceiverUserID = SignaloBotTestParameters.ExistingUserID,
                ReceiverAddress = "fake@aa.aa",
                ReceiverDisplayName = "receiver display name",

                SenderAddress = "fake@aa.aa",
                SenderDisplayName = "sender display name",

                MessageSubject = "subject text",
                MessageBody = "body text",
                IsBodyHtml = false,

                SendDateUtc = DateTime.UtcNow,
                IsDelayed = false,
                FailedAttempts = 10
            };
        }


       
    }
}
