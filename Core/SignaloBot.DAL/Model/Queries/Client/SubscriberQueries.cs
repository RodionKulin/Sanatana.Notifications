using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Enums;
using Common.EntityFramework.SafeCall;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.EntityFramework;
using SignaloBot.DAL.Model.Queries.QueryCreator;
using SignaloBot.DAL.Entities.Results;
using SignaloBot.DAL.Entities.Parameters;

namespace SignaloBot.DAL.Queries.Client
{
    public class SubscriberQueries : ISubscriberQueries 
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<ClientDbContext, UserDeliveryTypeSettings> _crud;

        
        //инициализация
        public SubscriberQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _crud = new EntityCRUD<ClientDbContext, UserDeliveryTypeSettings>(nameOrConnectionString, prefix);
        }



        //выбрать подписчиков
        public virtual List<Subscriber> Select(SubscriberParameters parameters, out Exception exception)
        {
            List<Subscriber> subscribers = null;

            _crud.DbSafeCallAndDispose((context) =>
            {
                var subscribersQueryCreator = new SubscriberQueryCreator();
                IQueryable<Subscriber> query = subscribersQueryCreator.CreateQuery(parameters, context);
                subscribers = query.ToListNoLock();
            }, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (subscribers == null)
            {
                subscribers = new List<Subscriber>();
            }

            return subscribers;
        }
    }
}
