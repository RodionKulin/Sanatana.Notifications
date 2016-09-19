using Common.EntityFramework;
using Common.MongoDb;
using Common.TestUtility;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.TestParameters.Model
{
    public static class SignaloBotTestParameters
    {
        //свойства
        public static string ConnectionString { get; set; }
        public static string SqlPrefix { get; set; }
        public static SqlConnetionSettings SqlConnetion { get; set; }
        public static MongoDbConnectionSettings MongoDbConnection { get; set; }

        public static Guid ExistingUserID { get; set; }
        public static ObjectId ExistingUserObjectId { get; set; }
        public static int ExistingDeliveryType { get; set; }
        public static int ExistingCategoryID { get; set; }
        public static string ExistingSubscriptionTopicID { get; set; }
        public static int ExistingReceivePeriodsGroupID { get; set; }


        //инициализация
        static SignaloBotTestParameters()
        {
            ConfigParametersLoader loader = ParametersUtility.GetConfigLoader();

            //connection
            ConnectionString = loader.ReadConnectionString("DefaultConnection");            
            SqlPrefix = loader.ReadAppSettingsValue("SqlPrefix");
            SqlConnetion = new SqlConnetionSettings(ConnectionString, SqlPrefix);
            MongoDbConnection = new MongoDbConnectionSettings()
            {
                DatabaseName = loader.ReadAppSettingsValue("MongoDbDatabase"),
                Host = loader.ReadAppSettingsValue("MongoDbHost"),
                Port = int.Parse(loader.ReadAppSettingsValue("MongoDbPort")),
                Login = loader.ReadAppSettingsValue("MongoDbLogin"),
                Password = loader.ReadAppSettingsValue("MongoDbPassword")
            };

            
            //SignaloBot.Client.Tests common parameters
            ExistingUserID = new Guid("A3D44032-40F8-44C0-89C4-FB9393B81FCE");
            ExistingUserObjectId = new ObjectId("56dad7d5bda87249c45cf5f1");
            ExistingDeliveryType = 0;
            ExistingCategoryID = 1;
            ExistingSubscriptionTopicID = "1";
            ExistingReceivePeriodsGroupID = 1;
        }


        //методы
        public static TKey GetTKey<TKey>()
            where TKey : struct
        {
            if (typeof(TKey) == typeof(Guid))
                return (TKey)Convert.ChangeType(Guid.NewGuid(), typeof(TKey));

            if (typeof(TKey) == typeof(ObjectId))
                return (TKey)Convert.ChangeType(ObjectId.GenerateNewId(), typeof(TKey));

            throw new NotImplementedException($"Type of {typeof(TKey)} is not supported");
        }

        public static TKey GetExistingUserID<TKey>()
            where TKey : struct
        {
            if (typeof(TKey) == typeof(Guid))
                return (TKey)Convert.ChangeType(SignaloBotTestParameters.ExistingUserID, typeof(TKey));

            if (typeof(TKey) == typeof(ObjectId))
                return (TKey)Convert.ChangeType(SignaloBotTestParameters.ExistingUserObjectId, typeof(TKey));

            throw new NotImplementedException($"Type of {typeof(TKey)} is not supported");
        }
    }
}
