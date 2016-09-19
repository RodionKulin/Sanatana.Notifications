using Common.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;
using SignaloBot.Initializer.Resources;
using SignaloBot.DAL.MongoDb;

namespace SignaloBot.Initializer.MongoDb
{
    public class CreateMongoDbIndexModule : IInitializeModule
    {
        //поля
        protected MongoDbConnectionSettings _connetionSettings;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public CreateMongoDbIndexModule(MongoDbConnectionSettings connetionSettings)
        {
            _connetionSettings = connetionSettings;
        }


        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.MongoDbIndex_Intro;
        }

        public Task Execute()
        {
            var initializer = new SignaloBotMongoDbInitializer(_connetionSettings);
            initializer.CreateAllIndexes(true);            

            return Task.FromResult(true);
        }

    }
}
