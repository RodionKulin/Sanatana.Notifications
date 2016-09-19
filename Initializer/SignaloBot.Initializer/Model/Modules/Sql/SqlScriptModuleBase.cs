using Common.EntityFramework;
using Common.EntityFramework.Scripts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Initializer
{
    public abstract class SqlScriptModuleBase
    {
        //поля
        protected SqlConnetionSettings _connection;

        
        
        //инициализация
        public SqlScriptModuleBase(SqlConnetionSettings connection)
        {
            _connection = connection;
        }



        //методы
        protected void InstallScripts<TDbContext>(Type scriptsType, bool createTables)
            where TDbContext : DbContext
        {
            var initializer = new ScriptInitializer<TDbContext>(scriptsType, _connection.Prefix);
            initializer.ScriptManager.ThrowOnUnknownScriptTypes = false;

            using (TDbContext context = (TDbContext)Activator.CreateInstance(typeof(TDbContext)
                , initializer, _connection.NameOrConnectionString, _connection.Prefix))
            {
                bool dbExists = context.Database.Exists();
                if (dbExists)
                {
                    if(createTables)
                    {
                        ScriptExtractor.ExtractFromDbContext(context);  //manual create tables
                    }
                    initializer.ExecuteScripts(context);            //manual create procedures
                }
                else
                {
                    context.Database.Initialize(true);              //auto create db, tables, procedures
                    //context.Database.ExecuteSqlCommand("SELECT 1");
                }
            }
        }
    }
}
