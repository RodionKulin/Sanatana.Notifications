using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Context;
using SignaloBot.DAL.Enums;
using System.Data.SqlClient;
using SignaloBot.DAL.Queries.Client;
using Common.Utility;
using Common.EntityFramework.SafeCall;
using Common.EntityFramework;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.DAL.Queries.Client
{
    public class UserReceivePeriodQueries : IUserReceivePeriodQueries
    {
         //поля
        protected string _prefix;
        protected ICommonLogger _logger;
        protected EntityCRUD<ClientDbContext, UserReceivePeriod> _crud;



        //инициализация
        public UserReceivePeriodQueries(string nameOrConnectionString, string prefix = null, ICommonLogger logger = null)
        {
            _prefix = prefix;
            _logger = logger;
            _crud = new EntityCRUD<ClientDbContext, UserReceivePeriod>(nameOrConnectionString, prefix);           
        }
                


        //методы
        public virtual void Rewrite(Guid userID, int periodsDeliveryType, int periodsCategoryID,
            List<UserReceivePeriod> periods, out Exception exception)
        {
            foreach (UserReceivePeriod item in periods)
            {
                item.PeriodBegin = SqlUtility.ToSqlTime(item.PeriodBegin);
                item.PeriodEnd = SqlUtility.ToSqlTime(item.PeriodEnd);
            }

            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", periodsDeliveryType);
            SqlParameter categoryIDParam = new SqlParameter("@periodsCategoryID", periodsCategoryID);
            SqlParameter periodsParam = CoreTVP.ToUserReceivePeriodType("@Periods", periods, _prefix);


            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format("EXEC {0}UserReceivePeriods_Rewrite @UserID, @DeliveryType, @periodsCategoryID, @Periods",
                   _prefix);
                context.Database.ExecuteSqlCommand(command, userIDParam
                    , deliveryTypeParam, categoryIDParam, periodsParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void Insert(UserReceivePeriod receivePeriod, out Exception exception)
        {
            receivePeriod.PeriodBegin = SqlUtility.ToSqlTime(receivePeriod.PeriodBegin);
            receivePeriod.PeriodEnd = SqlUtility.ToSqlTime(receivePeriod.PeriodEnd);

            _crud.Insert(receivePeriod, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }



        //delete
        public virtual void Delete(UserReceivePeriod receivePeriod, out Exception exception)
        {
            _crud.Delete(receivePeriod, out exception);

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
DELETE {0}UserReceivePeriods
WHERE UserID = @UserID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }

        public virtual void DeleteCategory(Guid userID, int periodsDeliveryType, int periodsCategoryID, out Exception exception)
        {
            SqlParameter userIDParam = new SqlParameter("@UserID", userID);
            SqlParameter deliveryTypeParam = new SqlParameter("@DeliveryType", periodsDeliveryType);
            SqlParameter categoryIDParam = new SqlParameter("@CategoryID", periodsCategoryID);

            _crud.DbSafeCallAndDispose((context) =>
            {
                string command = string.Format(@"
DELETE {0}UserReceivePeriods
WHERE UserID = @UserID
AND DeliveryType = @DeliveryType
AND CategoryID = @CategoryID", _prefix);

                context.Database.ExecuteSqlCommand(command, userIDParam, deliveryTypeParam, categoryIDParam);
            }
            , out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }



        //update
        public virtual void Update(UserReceivePeriod receivePeriod, out Exception exception)
        {
            receivePeriod.PeriodBegin = SqlUtility.ToSqlTime(receivePeriod.PeriodBegin);
            receivePeriod.PeriodEnd = SqlUtility.ToSqlTime(receivePeriod.PeriodEnd);

            _crud.Update(receivePeriod, out exception);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }
        }



        //select
        public virtual List<UserReceivePeriod> SelectAll(Guid userID, out Exception exception)
        {
            List<UserReceivePeriod> result = _crud.SelectAll(out exception, p => p.UserID == userID);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        public virtual List<UserReceivePeriod> SelectCategory(Guid userID
            , int periodsDeliveryType, int periodsCategoryID, out Exception exception)
        {
            List<UserReceivePeriod> result = _crud.SelectAll(out exception,
                p => p.UserID == userID
                && p.CategoryID == periodsCategoryID
                && p.DeliveryType == periodsDeliveryType);

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }

        public virtual List<UserReceivePeriod> SelectCategory(List<Guid> userIDs
            , int periodsDeliveryType, int periodsCategoryID, out Exception exception)
        {
            List<UserReceivePeriod> result = _crud.SelectAll(out exception,
                 p => p.CategoryID == periodsCategoryID
                     && p.DeliveryType == periodsDeliveryType
                     && userIDs.Contains(p.UserID));

            if (_logger != null && exception != null)
            {
                _logger.Exception(exception);
            }

            return result;
        }

    }
}
