using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Context;
using Common.Utility;
using SignaloBot.DAL.Entities.Core;
using System.Data.SqlClient;
using SignaloBot.DAL.Queries.Client;
using Common.EntityFramework.SafeCall;
using SignaloBot.DAL.Entities.Results;

namespace SignaloBot.DAL.Queries.Client
{
    public class UserDeliveryTypeSettingsQueries : IUserDeliveryTypeSettingsQueries 
    {
        //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<ClientDbContext, UserDeliveryTypeSettings> _crud;
        

        //инициализация
        public UserDeliveryTypeSettingsQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
             _logger = logger;
             _crud = new EntityCRUD<ClientDbContext, UserDeliveryTypeSettings>(nameOrConnectionString, prefix);
        }
                

        //insert
        public virtual void Insert(UserDeliveryTypeSettings userDeliveryTypeSettings, out Exception exception)
        {
            _crud.Insert(userDeliveryTypeSettings, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
        

        //delete
        public virtual void Delete(Guid userID, int deliveryType, out Exception exception)
        {
            UserDeliveryTypeSettings settingsTemp = new UserDeliveryTypeSettings()
            {
                UserID = userID,
                DeliveryType = deliveryType
            };

            _crud.Delete(settingsTemp, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void DeleteAll(Guid userID, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
           
            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}UserDeliveryTypeSettings
WHERE UserID = @UserID", _prefix);
                
                context.Database.ExecuteSqlCommand(command, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
        

        //update
        public virtual void Update(UserDeliveryTypeSettings userDeliveryTypeSettings, out Exception exception)
        {
            _crud.Update(userDeliveryTypeSettings, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void UpdateAddress(Guid userID, int deliveryType, string address, out Exception exception)
        {
            UserDeliveryTypeSettings settingsTemp = new UserDeliveryTypeSettings()
            {
                UserID = userID,
                DeliveryType = deliveryType,

                Address = address
            };

            _crud.Update(settingsTemp, out exception, p => p.Address);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
        
        public virtual void UpdateLastVisit(Guid userID, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
           
            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
UPDATE {0}UserDeliveryTypeSettings
SET LastUserVisitUtc = GETUTCDATE()
WHERE UserID = @UserID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void UpdateTimeZone(Guid userID, TimeZoneInfo timeZone, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter timeZoneParam = new SqlParameter("@TimeZoneID", timeZone.Id);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
UPDATE {0}UserDeliveryTypeSettings
SET TimeZoneID = @TimeZoneID
WHERE UserID = @UserID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam, timeZoneParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void ResetNDRCount(Guid userID, int deliveryType, out Exception exception)
        {
            UserDeliveryTypeSettings settingsTemp = new UserDeliveryTypeSettings()
            {
                UserID = userID,
                DeliveryType = deliveryType,

                NDRCount = 0,
                IsBlockedOfNDR = false,
                BlockOfNDRResetCode = null,
                BlockOfNDRResetCodeSendDateUtc = null
            };

            _crud.Update(settingsTemp
                , out exception
                , p => p.NDRCount, p => p.IsBlockedOfNDR, p => p.BlockOfNDRResetCode, p => p.BlockOfNDRResetCodeSendDateUtc);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void UpdateNDRResetCode(Guid userID, int deliveryType, string resetCode, out Exception exception)
        {
            UserDeliveryTypeSettings settingsTemp = new UserDeliveryTypeSettings()
            {
                UserID = userID,
                DeliveryType = deliveryType,

                BlockOfNDRResetCode = resetCode,
                BlockOfNDRResetCodeSendDateUtc = DateTime.UtcNow
            };

            _crud.Update(settingsTemp, out exception
                , p => p.BlockOfNDRResetCode
                , p => p.BlockOfNDRResetCodeSendDateUtc);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void DisableAllDeliveryTypes(Guid userID, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
UPDATE {0}UserDeliveryTypeSettings
SET IsEnabled = 0
WHERE UserID = @UserID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }
                

        //select
        public virtual List<UserDeliveryTypeSettings> SelectAllUserDeliveryTypes(Guid userID, out Exception exception)
        {
            List<UserDeliveryTypeSettings> result = _crud.SelectAll(out exception, p => p.UserID == userID);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        public virtual UserDeliveryTypeSettings Find(Guid userID, int deliveryType, out Exception exception)
        {
            UserDeliveryTypeSettings result = _crud.SelectFirst(
                e => e.UserID == userID && e.DeliveryType == deliveryType
                , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        public virtual bool CheckAddressExists(string address, int deliveryType, out Exception exception)
        {
            bool exist = false;

            SqlParameter addressParam = new SqlParameter("@Address", address);
            SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", deliveryType);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}Address_Exists @Address, @DeliveryType", _prefix);

                int result = context.Database.SqlQuery<int>(command
                    , addressParam, deliveryTypeParam)
                    .FirstOrDefault();
                exist = result == 1;
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return exist;
        }

        public virtual List<UserDeliveryTypeSettings> SelectAllDeliveryTypes(int start, int end
            , out int total, out Exception exception)
        {
            List<UserDeliveryTypeSettingsTotal> list = null;
                  
            SqlParameter firstIndexParam = new SqlParameter("@FirstIndex", start);
            SqlParameter lastIndexParam = new SqlParameter("@LastIndex", end);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}UserDeliveryTypeSettings_SelectAll @FirstIndex, @LastIndex"
                   , _prefix);
                DbRawSqlQuery<UserDeliveryTypeSettingsTotal> query = context.Database.SqlQuery<UserDeliveryTypeSettingsTotal>(
                    command, firstIndexParam, lastIndexParam);
                list = query.ToList();
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            if (list == null)
                list = new List<UserDeliveryTypeSettingsTotal>();

            total = list.Count > 0 ? (int)list[0].TotalRows : 0;

            return list.Select(p => (UserDeliveryTypeSettings)p).ToList();
        }
    }
}
