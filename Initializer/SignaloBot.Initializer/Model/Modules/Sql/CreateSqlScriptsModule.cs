using Common.EntityFramework;
using Common.EntityFramework.Scripts;
using Common.Initializer;
using SignaloBot.DAL.SQL;
using SignaloBot.DAL.SQL.Resources;
using SignaloBot.Initializer.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Initializer
{
    public class CreateSqlScriptsModule : SqlScriptModuleBase, IInitializeModule
    {
        //события
        public event ProgressDelegate ProgressUpdated;

        
        //инициализация
        public CreateSqlScriptsModule(SqlConnetionSettings connection)
            : base(connection)
        {
        }



        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.CoreScripts_Intro;
        }

        public Task Execute()
        {
            InstallScripts<ClientDbContext>(typeof(ClientScripts), true);            
            InstallScripts<SenderDbContext>(typeof(SenderScripts), true);            
            InstallScripts<NDRDbContext>(typeof(NDRScripts), true);

            return Task.FromResult(true);
        }


    
    }
}
