using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb
{
    public static class IEnumerableExtensions
    {
        public static string Join(this IEnumerable<string> values, string separator)
        {
            if (values == null)
            {
                return "";
            }

            return string.Join(separator, values);
        }

        public static string Join(this IEnumerable<ObjectId> values, string separator)
        {
            if(values == null)
            {
                return "";
            }

            return string.Join(separator, values);
        }
    }
}
