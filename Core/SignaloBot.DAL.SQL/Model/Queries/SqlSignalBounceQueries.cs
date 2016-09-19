using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using Common.Utility;
using Common.EntityFramework;

namespace SignaloBot.DAL.SQL
{
    public class SqlSignalBounceQueries : ISignalBounceQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;
        
        
        //инициализация
        public SqlSignalBounceQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }


        //методы
        public virtual Task<bool> Insert(List<SignalBounce<Guid>> messages)
        {
            bool result = false;

            using (NDRDbContext context = new NDRDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    context.BulkInsert(messages);
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            return Task.FromResult(result);
        }

        
    }
}
