using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Scripts;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Resources;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Context
{
    public class SenderDbContextFactory : ISenderDbContextFactory
    {
        //fields
        protected SqlConnectionSettings _connectionSettings;

        
        //init
        public SenderDbContextFactory(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        //methods
        public virtual SenderDbContext GetDbContext()
        {
            return new SenderDbContext(_connectionSettings);
        }

        public virtual void InitializeDatabase()
        {
            var scriptReplacements = new Dictionary<string, string>()
            {
                { "pref", _connectionSettings.Schema }
            };
            var scriptInitializer = new ScriptInitializer(typeof(InstallScripts), scriptReplacements);
            scriptInitializer.ScriptManager.ThrowOnUnknownScriptTypes = false;

            using (SenderDbContext context = GetDbContext())
            {
                var creator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
                bool dbExists = creator.Exists();
                if (dbExists)
                {
                    bool tableExists = CheckTableExists(context);
                    if (tableExists == false)
                    {
                        ScriptExtractor.ExtractFromDbContext(context);
                        scriptInitializer.ExecuteScripts(context);
                    }
                }
                else
                {
                    context.Database.EnsureCreated();                    
                    scriptInitializer.ExecuteScripts(context);
                }
            }
        }

        public virtual bool CheckTableExists(SenderDbContext context)
        {
            string sql = $@"
                SELECT 1 FROM sys.tables AS T
                INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                WHERE S.Name = '{_connectionSettings.Schema}' AND T.Name = '{DefaultTableNameConstants.DispatchTemplates}'";

            using (DbConnection connection = context.Database.GetDbConnection())
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    object result = command.ExecuteScalar();
                    int intResult = (int)result;
                    bool boolresult = intResult > 0;
                    return boolresult;
                }
            }
        }

    }
}
