using Common.Initializer;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SignaloBot.Initializer.MongoDb;
using SignaloBot.DAL.MongoDb;
using MongoDB.Bson;
using SignaloBot.DAL;
using SignaloBot.DAL.SQL;
using Common.Utility;

namespace SignaloBot.Initializer
{
    class Program
    {
        static void Main(string[] args)
        {
            InstallMongoDb();
        }

        private static void InstallMongoDb()
        {
            var initializer = new InitializeManager();

            initializer.RegisterPrintable(SignaloBotTestParameters.MongoDbConnection
                , SignaloBotTestParameters.MongoDbConnection.ToDetailsString());

            initializer.Builder.RegisterType<ShoutExceptionLogger>().As<ICommonLogger>();
            
            initializer.Builder.RegisterType<MongoDbUserDeliveryTypeSettingsQueries>().As<IUserDeliveryTypeSettingsQueries<ObjectId>>();
            initializer.Builder.RegisterType<MongoDbUserCategorySettingsQueries>().As<IUserCategorySettingsQueries<ObjectId>>();
            initializer.Builder.RegisterType<MongoDbUserTopicSettingsQueries>().As<IUserTopicSettingsQueries<ObjectId>>();

            initializer.RegisterModules(new List<Type>()
            {
                typeof(DropMongoDbModule),
                typeof(CreateMongoDbIndexModule),
                typeof(TestDataModule<ObjectId>),
                //typeof(MongoDbLoadTestDataModule)
            });

            initializer.Initialize();
        }        
        private static void InstallSql()
        {
            var initializer = new InitializeManager();

            initializer.RegisterPrintable(SignaloBotTestParameters.SqlConnetion
                , SignaloBotTestParameters.SqlConnetion.ToDetailsString());

            initializer.Builder.RegisterType<ShoutExceptionLogger>().As<ICommonLogger>();

            initializer.Builder.RegisterType<SqlUserDeliveryTypeSettingsQueries>().As<IUserDeliveryTypeSettingsQueries<Guid>>();
            initializer.Builder.RegisterType<SqlUserCategorySettingsQueries>().As<IUserCategorySettingsQueries<Guid>>();
            initializer.Builder.RegisterType<SqlUserTopicSettingsQueries>().As<IUserTopicSettingsQueries<Guid>>();
            
            initializer.RegisterModules(new List<Type>()
            {
                typeof(DropSqlDbModule),
                typeof(CreateSqlScriptsModule),
                typeof(CreateNotificationSqlScriptsModule),
                typeof(TestDataModule<Guid>),
            });

            initializer.Initialize();

        }
    }
}
