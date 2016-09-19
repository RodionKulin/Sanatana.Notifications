using Common.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;
using SignaloBot.DAL.MongoDb;
using MongoDB.Driver;
using SignaloBot.Initializer.Resources;

namespace SignaloBot.Initializer.MongoDb
{
    public class DropMongoDbModule : IInitializeModule
    {
        //поля
        private MongoDbConnectionSettings _settings;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public DropMongoDbModule(MongoDbConnectionSettings settings)
        {
            _settings = settings;
        }


        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.DropMongoDb_Intro;
        }

        public Task Execute()
        {
            var mongoContext = new SignaloBotMongoDbContext(_settings);
            IMongoDatabase mainDb = mongoContext.SignalDispatches.Database;
            return mainDb.Client.DropDatabaseAsync(mainDb.DatabaseNamespace.DatabaseName);
        }

    }
}
