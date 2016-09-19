using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using System.Data.SqlClient;
using Common.EntityFramework;

namespace SignaloBot.DAL.SQL
{
    public class CoreTVP
    {
        //имена параметров
        public const string UPDATE_USERS_PARAMETER_NAME = "@UpdateUsers";

        //табличные типы
        public const string GUID_TYPE = "GuidType";
        public const string INT_TYPE = "IntType";
        public const string CATEGORY_TYPE = "CategoryType";
        public const string SIGNAL_SEND_DATE_TYPE = "SignalSendDateType";
        public const string RECEIVE_PERIOD_TYPE = "UserReceivePeriodType";
        public const string UPDATE_USER_TYPE = "UpdateUserType";


        //методы       
        public static SqlParameter ToGuidType(string paramName, List<Guid> items, string prefix)
        {
            items = items.Distinct().ToList();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(Guid));

            foreach (Guid item in items)
            {
                object[] values = new object[] { item };
                dataTable.Rows.Add(values);
            }

            SqlParameter param = new SqlParameter(paramName, dataTable);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = prefix + GUID_TYPE;

            return param;
        }

        public static SqlParameter ToIntType(string paramName, List<int> items, string prefix)
        {
            items = items.Distinct().ToList();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));

            foreach (int item in items)
            {
                object[] values = new object[] { item };
                dataTable.Rows.Add(values);
            }

            SqlParameter param = new SqlParameter(paramName, dataTable);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = prefix + INT_TYPE;

            return param;
        }

        public static SqlParameter ToUserReceivePeriodType(string paramName, List<UserReceivePeriod<Guid>> items, string prefix)
        {
            DataTable dataTable = items.ToDataTable(new List<string>()
            {
                ReflectionExtensions.GetPropertyName((UserReceivePeriod<Guid> t) => t.PeriodOrder),
                ReflectionExtensions.GetPropertyName((UserReceivePeriod<Guid> t) => t.PeriodBegin),
                ReflectionExtensions.GetPropertyName((UserReceivePeriod<Guid> t) => t.PeriodEnd)
            });

            SqlParameter param = new SqlParameter(paramName, dataTable);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = prefix + RECEIVE_PERIOD_TYPE;

            return param;
        }

        public static SqlParameter ToUpdateUserType(string paramName, List<SignalDispatchBase<Guid>> items, string prefix)
        {
            var groupedMessages = items.Where(p => p.ReceiverUserID != null)
               .GroupBy(p => new { p.ReceiverUserID, p.DeliveryType, p.CategoryID, p.TopicID })
               .Select(p => new UpdateUserParameter
               {
                   UserID = p.First().ReceiverUserID.Value,
                   SendCount = p.Count(),
                   DeliveryType = p.First().DeliveryType,
                   CategoryID = p.First().CategoryID,
                   TopicID = p.First().TopicID
               });

            DataTable dataTable = groupedMessages.ToDataTable(new List<string>()
            {
                ReflectionExtensions.GetPropertyName((UpdateUserParameter t) => t.UserID),
                ReflectionExtensions.GetPropertyName((UpdateUserParameter t) => t.SendCount),
                ReflectionExtensions.GetPropertyName((SignalDispatchBase<Guid> t) => t.DeliveryType),
                ReflectionExtensions.GetPropertyName((SignalDispatchBase<Guid> t) => t.CategoryID),
                ReflectionExtensions.GetPropertyName((SignalDispatchBase<Guid> t) => t.TopicID)
            });

            SqlParameter param = new SqlParameter(paramName, dataTable);
            param.SqlDbType = SqlDbType.Structured;
            param.TypeName = prefix + UPDATE_USER_TYPE;

            return param;
        }

    }
}
