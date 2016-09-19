using SignaloBot.TestParameters.Model;
using Common.EntityFramework;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL;
using SignaloBot.DAL.SQL;
using Common.Initializer;
using SignaloBot.Initializer.Resources;
using MongoDB.Bson;

namespace SignaloBot.Initializer
{
    public class TestDataModule<TKey> : IInitializeModule
        where TKey : struct
    {
        //поля
        private IUserDeliveryTypeSettingsQueries<TKey> _deliveryTypeQueries;
        private IUserCategorySettingsQueries<TKey> _categoryQueries;
        private IUserTopicSettingsQueries<TKey> _topicQueries;

        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public TestDataModule(IUserDeliveryTypeSettingsQueries<TKey> deliveryTypeQueries
            , IUserCategorySettingsQueries<TKey> categoryQueries
            , IUserTopicSettingsQueries<TKey> topicQueries)
        {
            _deliveryTypeQueries = deliveryTypeQueries;
            _categoryQueries = categoryQueries;
            _topicQueries = topicQueries;
        }


        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.TestData_Intro;
        }

        public async Task Execute()
        {
            TKey userID = SignaloBotTestParameters.GetExistingUserID<TKey>();
          
            var dtSettings = new List<UserDeliveryTypeSettings<TKey>>()
            {
                UserDeliveryTypeSettings<TKey>.Default(userID
                    , SignaloBotTestParameters.ExistingDeliveryType, "mail@test.ml", "ru"),
                UserDeliveryTypeSettings<TKey>.Default(SignaloBotTestParameters.GetTKey<TKey>()
                    , SignaloBotTestParameters.ExistingDeliveryType, "mail2@test.ml", "ru"),
                UserDeliveryTypeSettings<TKey>.Default(SignaloBotTestParameters.GetTKey<TKey>()
                    , SignaloBotTestParameters.ExistingDeliveryType, "mail3@test.ml", "ru")
            };
            dtSettings[0].LastUserVisitUtc = DateTime.UtcNow.AddSeconds(5);
            bool result = await _deliveryTypeQueries.Insert(dtSettings);

            var categorySettings = new List<UserCategorySettings<TKey>>()
            {
                SignaloBotEntityCreator<TKey>.CreateUserCategorySettings(userID, SignaloBotTestParameters.ExistingCategoryID),
                SignaloBotEntityCreator<TKey>.CreateUserCategorySettings(userID, 100),
                SignaloBotEntityCreator<TKey>.CreateUserCategorySettings(userID, 101)
            };
            categorySettings[0].LastSendDateUtc = categorySettings[0].LastSendDateUtc.Value.AddDays(-1);
            categorySettings[1].LastSendDateUtc = categorySettings[1].LastSendDateUtc.Value.AddDays(-2);
            categorySettings[2].LastSendDateUtc = categorySettings[2].LastSendDateUtc.Value.AddDays(-3);
            result = await _categoryQueries.Insert(categorySettings);

            var topicSettings = new List<UserTopicSettings<TKey>>()
            {
               SignaloBotEntityCreator<TKey>.CreateUserTopicSettings(userID, categoryID: SignaloBotTestParameters.ExistingCategoryID, topicID: 1.ToString()),
               SignaloBotEntityCreator<TKey>.CreateUserTopicSettings(userID, categoryID: SignaloBotTestParameters.ExistingCategoryID, topicID: 2.ToString()),
               SignaloBotEntityCreator<TKey>.CreateUserTopicSettings(userID, categoryID: SignaloBotTestParameters.ExistingCategoryID, topicID: 3.ToString())
            };
            topicSettings[0].AddDateUtc = topicSettings[0].AddDateUtc.AddDays(-1);
            topicSettings[1].AddDateUtc = topicSettings[1].AddDateUtc.AddDays(-2);
            topicSettings[2].AddDateUtc = topicSettings[2].AddDateUtc.AddDays(-3);
            result = await _topicQueries.Insert(topicSettings);

        }
        

    }
}
