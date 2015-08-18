using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using Common.Utility;
using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Queries;
using SignaloBot.DAL;
using SignaloBot.DAL.Enums;
using SignaloBot.DAL.Entities.Core;
using Common.EntityFramework.SafeCall;

namespace SignaloBot.DAL.Queries.Sender
{
    public class QueueQueries : IQueueQueries<Signal>
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<SenderDbContext, Signal> _signalCrud;


        //инициализация
        public QueueQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _signalCrud = new EntityCRUD<SenderDbContext, Signal>(nameOrConnectionString, prefix);
        }


        //Select
        public virtual List<Signal> Select(int count, List<int> deliveryTypes, int maxFailedAttempts)
        {
            List<Signal> list = new List<Signal>();
            Exception exception;

            if (count == 0 || deliveryTypes == null || deliveryTypes.Count == 0)
            {
                return list;
            }

            _signalCrud.DbSafeCallAndDispose((context) =>
            {
                list = (from msg in context.Signals
                        orderby msg.SendDateUtc ascending                        
                        where msg.SendDateUtc <= DateTime.UtcNow
                            && deliveryTypes.Contains(msg.DeliveryType)
                            && msg.FailedAttempts < maxFailedAttempts
                        select msg)
                        .Take(count).ToList();
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<Signal>();

            return list;
        }


        //Delete
        public virtual void Delete(Signal message)
        {
            Exception exception;
            _signalCrud.Delete(message, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }       
        }

        public virtual void Delete(List<Signal> messages)
        {
            Exception exception;

            IEnumerable<Guid> messageIDs = messages.Select(p => p.SignalID).Distinct();
            string idString = string.Join(",", messageIDs);
            SqlParameter idsParam = new SqlParameter("@IDs", idString);

            _signalCrud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}Signals
WHERE SignalID IN @IDs", _prefix);

                context.Database.ExecuteSqlCommand(command, idsParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
        
        
        //Update
        public virtual void Update(Signal message)
        {
            Exception exception;
            _signalCrud.Update(message, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }


        //IDisposable
        public void Dispose()
        {

        }
    }
}
