using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using SignaloBot.DAL.Entities;
using Common.Utility;
using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Queries.NDR;
using Common.EntityFramework.SafeCall;
using Common.EntityFramework.Merge;

namespace SignaloBot.DAL.QueriesNDR
{
    public class NDRQueries : INDRQueries 
    { 
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<ClientDbContext, UserDeliveryTypeSettings> _settingsCrud;
        protected EntityCRUD<NDRDbContext, BouncedMessage> _rejectsCrud;
        
        
        //инициализация
        public NDRQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _settingsCrud = new EntityCRUD<ClientDbContext, UserDeliveryTypeSettings>(nameOrConnectionString, prefix);
            _rejectsCrud = new EntityCRUD<NDRDbContext, BouncedMessage>(nameOrConnectionString, prefix);
        }


        //BouncedMessage
        public virtual void BouncedMessage_Insert(List<BouncedMessage> messages)
        {
            Exception exception = null;
            if (messages.Count == 0)
                return;

            _rejectsCrud.DbSafeCallAndDispose((context) =>
            {
                context.BulkInsert(messages);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }


        
        //UserDeliveryTypeSettings
        public virtual List<UserDeliveryTypeSettings> NDRSettings_Select(int deliveryType, List<string> addresses)
        {
            if (addresses.Count == 0)
            {
                return new List<UserDeliveryTypeSettings>();
            }

            Exception exception = null;            
            List<UserDeliveryTypeSettings> result = _settingsCrud.SelectAll(out exception,
                p => p.DeliveryType == deliveryType && addresses.Contains(p.Address));
            
            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;

        }

        public virtual void NDRSettings_Update(List<UserDeliveryTypeSettings> settings)
        {
            Exception exception = null;
            string tvpName = _prefix + NDR_TVP.USER_NDR_SETTINGS_TYPE;

            _settingsCrud.DbSafeCallAndDispose((context) =>
            {
                MergeOperation<UserDeliveryTypeSettings> merge = context.Merge(settings, tvpName);

                merge.Source.IncludeProperty(p => p.UserID)
                    .IncludeProperty(p => p.DeliveryType)
                    .IncludeProperty(p => p.NDRCount)
                    .IncludeProperty(p => p.IsBlockedOfNDR);

                merge.Compare.IncludeProperty(p => p.UserID)
                    .IncludeProperty(p => p.DeliveryType);

                merge.Update.IncludeProperty(p => p.NDRCount)
                    .IncludeProperty(p => p.IsBlockedOfNDR);

                merge.Execute(MergeType.Update);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
    }
}
