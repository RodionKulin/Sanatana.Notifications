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
using Common.DataGenerator;
using Common.MongoDb;
using SignaloBot.DAL.MongoDb;

namespace SignaloBot.Initializer
{
    public class MongoDbLoadTestDataModule : IInitializeModule
    {
        //поля
        private int _deliveryTypesCount = 5000000; // 5kk
        protected SignaloBotMongoDbContext _context;


        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public MongoDbLoadTestDataModule(MongoDbConnectionSettings connetionSettings)
        {
            _context = new SignaloBotMongoDbContext(connetionSettings);
        }


        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.LoadTestData_Intro;
        }

        public Task Execute()
        {
            var fillTask = new FillTaskAssembler()
                .RegisterSingleResult(_deliveryTypesCount, _context.UserDeliveryTypeSettings, CreateDeliveryType)
                //.RegisterMultipleResult<UserCategorySettings<ObjectId>, UserDeliveryTypeSettings<ObjectId>>(1, _context.UserCategorySettings, CreateCategory)
                //.RegisterMultipleResult<UserTopicSettings<ObjectId>, UserDeliveryTypeSettings<ObjectId>>(1, _context.UserTopicSettings, CreateTopic)
                ;

            fillTask.Progress.ProgressUpdateEvent += Progress_ProgressUpdateEvent;
            FillResult result = fillTask.Fill().Result;

            return Task.FromResult<int>(0);
        }

        private void Progress_ProgressUpdateEvent(decimal progress)
        {
            int percentage = (int)(progress * 100);
            string message = string.Format(InnerMessages.LoadTestData_PercentMessage, percentage);

            if (ProgressUpdated != null)
                ProgressUpdated(message);
        }


        //методы генерации данных
        private UserDeliveryTypeSettings<ObjectId> CreateDeliveryType(
            GenerationContext context)
        {
            ObjectId userID = ObjectId.GenerateNewId();
            return SignaloBotEntityCreator<ObjectId>.CreateUserDeliveryTypeSettings(userID);
        }

        private List<UserCategorySettings<ObjectId>> CreateCategory(
            GenerationContext<UserDeliveryTypeSettings<ObjectId>> context)
        {
            ObjectId userID = context.ForeignEntity.UserID;
            var category = SignaloBotEntityCreator<ObjectId>.CreateUserCategorySettings(userID, 1);
            return new List<UserCategorySettings<ObjectId>>() { category };
        }

        private List<UserTopicSettings<ObjectId>> CreateTopic(
            GenerationContext<UserDeliveryTypeSettings<ObjectId>> context)
        {
            ObjectId userID = context.ForeignEntity.UserID;
            var topic = SignaloBotEntityCreator<ObjectId>.CreateUserTopicSettings(userID);
            return new List<UserTopicSettings<ObjectId>>() { topic };
        }


    }
}
