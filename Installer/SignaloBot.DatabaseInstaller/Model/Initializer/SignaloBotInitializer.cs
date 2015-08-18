using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Common.EntityFramework;
using SignaloBot.TestParameters.Model;
using SignaloBot.DatabaseInstaller.Seed;
using SignaloBot.DAL.Context;
using SignaloBot.DatabaseInstaller.Model.Initializer;
using Common.EntityFramework.Install;
using Common.EntityFramework.Install.Scripts;
using System.IO;
using SignaloBot.Notifications.Database;


namespace SignaloBot.DatabaseInstaller.Initializer
{

    public class SignaloBotInitializer
    {
        //публичный метод
        public void RunScripts(InitializeSettings settings)
        {            
            //удалить существующую базу
            if (settings.DropExistingDB)
            {
                DropDatabase(settings);
            }

            //установить таблицы и процедуры SignaloBot
            if (settings.InstallUserSettings)
            {
                Type scriptResource = typeof(SignaloBot.Client.Resources.ClientScripts);
                InstallScripts<ClientDbContext>(settings, scriptResource);
            }

            if (settings.InstallSendQueue)
            {
                Type scriptResource = typeof(SignaloBot.Sender.Resources.SenderScripts);
                InstallScripts<SenderDbContext>(settings, scriptResource);
            }

            if (settings.InstallNDR)
            {
                Type scriptResource = typeof(SignaloBot.NDR.Resources.NDRScripts);
                InstallScripts<NDRDbContext>(settings, scriptResource);
            }
                
            if (settings.InstallNotifications)
            {
                Type scriptResource = typeof(SignaloBot.Notifications.Resources.NotificationScripts);
                InstallScripts<NotificationsDbContext>(settings, scriptResource);
            }
                


            //добавить демо данные
            if (settings.InsertDemoData)
            {
                InsertDemoData(settings);
            }
        }
        

        //приватные методы
        private void DropDatabase(InitializeSettings settings)
        {
            using (ClientDbContext context = new ClientDbContext(settings.ConnectionString, settings.SqlPrefix))
            {
                context.Database.Delete();
            }
        }
                
        private void InstallScripts<TDbContext>(InitializeSettings settings, Type scriptsType)
            where TDbContext : DbContext
        {
            var initializer = new ScriptInitialiser<TDbContext>(scriptsType, settings.SqlPrefix);
            initializer.ThrowOnUnknownScriptTypes = false;

            using (TDbContext context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), initializer, settings.ConnectionString, settings.SqlPrefix))
            {
                bool dbExists = context.Database.Exists();
                if (dbExists)
                {
                    ScriptExtractor.ExtractFromDbContext(context);
                    initializer.ExecuteScripts(context);
                }
                else
                {
                    context.Database.Initialize(true);
                    //context.Database.ExecuteSqlCommand("SELECT 1");
                }
            }
        }

        private void InsertDemoData(InitializeSettings settings)
        {
            using (var context = new ClientDbContext(settings.ConnectionString, settings.SqlPrefix))
            {
                var testDataInitializer = new TestSeedCoreInitializer();
                testDataInitializer.InsertContent(context);
            }
        }
    }
}
