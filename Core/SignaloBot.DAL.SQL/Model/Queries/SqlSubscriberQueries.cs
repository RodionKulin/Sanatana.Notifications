using Common.EntityFramework;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    public class SqlSubscriberQueries : ISubscriberQueries<Guid>
    {
        //поля
        protected SqlConnetionSettings _settings;
        protected ICommonLogger _logger;

        
        //инициализация
        public SqlSubscriberQueries(ICommonLogger logger, SqlConnetionSettings connectionSettings)
        {
            _logger = logger;
            _settings = connectionSettings;
        }



        //выбрать подписчиков
        public virtual async Task<QueryResult<List<Subscriber<Guid>>>> Select(
            SubscribtionParameters parameters, UsersRangeParameters<Guid> usersRange)
        {
            List<Subscriber<Guid>> subscribers = null;
            bool result = false;

            using (ClientDbContext context = new ClientDbContext(_settings.NameOrConnectionString, _settings.Prefix))
            {
                try
                {
                    var subscribersQueryCreator = new SubscriberQueryCreator();
                    IQueryable<Subscriber<Guid>> query = 
                        subscribersQueryCreator.CreateQuery(parameters, usersRange, context);

                    subscribers = await query.ToListAsync();
                    result = true;
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }

            if (subscribers == null)
            {
                subscribers = new List<Subscriber<Guid>>();
            }

            return new QueryResult<List<Subscriber<Guid>>>(subscribers, !result);
        }
    }
}
