using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Queries.QueryCreator;
using Common.EntityFramework.Merge;
using Common.EntityFramework.SafeCall;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using SignaloBot.DAL.Entities.Parameters;
using EntityFramework.BulkInsert.Helpers;
using SignaloBot.DAL.Model.Queries;

namespace SignaloBot.DAL.Queries.Client
{
    public class SignalQueries : ISignalQueries
    {
         //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<SenderDbContext, Signal> _crud;
        protected DbAssistant<ClientDbContext> _coreContext;


        //инициализация
        public SignalQueries(string nameOrConnectionString, string prefix = null
            , ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _crud = new EntityCRUD<SenderDbContext, Signal>(nameOrConnectionString, prefix);
            _coreContext = new DbAssistant<ClientDbContext>(nameOrConnectionString, prefix);
        }

        
        //методы
        public virtual void Insert(List<Signal> messages, out Exception exception)
        {
            _crud.DbSafeCallAndDispose((context) =>
            {
                context.BulkInsert(messages);               
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual List<Signal> SelectDelayed(Guid userID, int deliveryType, int categoryID, out Exception exception)
        {
            List<Signal> result = _crud.SelectAll(out exception,
                p => p.ReceiverUserID == userID 
                    && p.IsDelayed == true
                    && p.DeliveryType == deliveryType
                    && p.CategoryID == categoryID);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (result == null)
            {
                result = new List<Signal>();
            }

            return result;
        }

        public virtual List<Signal> SelectDelayed(Guid userID, int deliveryType, out Exception exception)
        {
            List<Signal> result = _crud.SelectAll(out exception,
                p => p.ReceiverUserID == userID
                    && p.IsDelayed == true
                    && p.DeliveryType == deliveryType);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (result == null)
            {
                result = new List<Signal>();
            }

            return result;
        }

        public virtual void UpdateSendDateUtc(List<Signal> messages, out Exception exception)
        {
            string tvpName = _prefix + CoreTVP.SIGNAL_SEND_DATE_TYPE;

            _crud.DbSafeCallAndDispose((context) =>
            {
                MergeOperation<Signal> upsert = context.Merge(messages, tvpName);

                upsert.Source.IncludeProperty(p => p.SignalID)
                    .IncludeProperty(p => p.SendDateUtc)
                    .IncludeProperty(p => p.IsDelayed);

                upsert.Compare.IncludeProperty(p => p.SignalID);

                upsert.Update.IncludeProperty(p => p.SendDateUtc)
                    .IncludeProperty(p => p.IsDelayed);

                upsert.Execute(MergeType.Update);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void UpdateCounters(UpdateParameters parameters
            , List<Signal> messages, out Exception exception)
        {
            if (!parameters.UpdateAnything)
            {
                exception = null;
                return;
            }
            
            SqlParameter updateUsersParam = CoreTVP.ToUpdateUserType(CoreTVP.UPDATE_USERS_PARAMETER_NAME, messages, _prefix);
            
            _coreContext.DbSafeCallAndDispose((context) =>
            {
                var scriptCreator = new UpdateCountersQueryCreator();
                string command = scriptCreator.CreateQuery(parameters, context, _prefix);

                context.Database.ExecuteSqlCommand(command, updateUsersParam);
            }, out exception);
            
            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

    }
}
