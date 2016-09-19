using Common.EntityFramework;
using Common.EntityFramework.Scripts;
using Common.Initializer;
using SignaloBot.DAL.SQL;
using SignaloBot.DAL.SQL.Resources;
using SignaloBot.Initializer.Resources;
using SignaloBot.WebNotifications.Database;
using SignaloBot.WebNotifications.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Initializer
{
    public class CreateNotificationSqlScriptsModule : SqlScriptModuleBase, IInitializeModule
    {
        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public CreateNotificationSqlScriptsModule(SqlConnetionSettings connection)
            : base(connection)
        {
        }



        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.NotificationScripts_Intro;
        }

        public Task Execute()
        {
            InstallScripts<NotificationsDbContext>(typeof(WebNotificationScripts), true);            
            return Task.FromResult(true);
        }


    }
}
