using SignaloBot.DAL;
using SignaloBot.TestParameters.Model;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace SignaloBot.TestParameters.Model
{
    public class SignaloBotEntityCreator<TKey>
        where TKey : struct
    {
        //методы
        public static UserDeliveryTypeSettings<TKey> CreateUserDeliveryTypeSettings(TKey userId)
        {
            return new UserDeliveryTypeSettings<TKey>()
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

        public static UserCategorySettings<TKey> CreateUserCategorySettings(TKey userId, int? categoryID = null)
        {
            categoryID = categoryID ?? SignaloBotTestParameters.ExistingCategoryID;

            return new UserCategorySettings<TKey>()
            {
                UserID = userId,
                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                CategoryID = categoryID.Value,
                LastSendDateUtc = DateTime.UtcNow,
                IsEnabled = true,
                SendCount = 0
            };
        }

        public static UserTopicSettings<TKey> CreateUserTopicSettings(TKey userID
            , int? categoryID = null, string topicID = null)
        {
            categoryID = categoryID ?? SignaloBotTestParameters.ExistingCategoryID;
            topicID = topicID ?? SignaloBotTestParameters.ExistingSubscriptionTopicID;

            return new UserTopicSettings<TKey>()
            {
                UserID = userID,
                CategoryID = categoryID.Value,
                TopicID = topicID,
                IsDeleted = false,
                IsEnabled = true,
                AddDateUtc = DateTime.UtcNow
            };
        }

        public static UserReceivePeriod<TKey> CreateUserReceivePeriod(int deliveryType, int order)
        {
            return new UserReceivePeriod<TKey>()
            {
                UserID = SignaloBotTestParameters.GetExistingUserID<TKey>(),
                ReceivePeriodsGroupID = SignaloBotTestParameters.ExistingReceivePeriodsGroupID,
                PeriodOrder = order,
                PeriodBegin = TimeSpan.FromHours(0),
                PeriodEnd = TimeSpan.FromHours(24)
            };
        }

        public static SubjectDispatch<TKey> CreateSignal()
        {
            return new SubjectDispatch<TKey>()
            {
                //SignalID = устанавливается в базе

                DeliveryType = SignaloBotTestParameters.ExistingDeliveryType,
                CategoryID = SignaloBotTestParameters.ExistingCategoryID,
                TopicID = SignaloBotTestParameters.ExistingSubscriptionTopicID,

                ReceiverUserID = SignaloBotTestParameters.GetExistingUserID<TKey>(),
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
