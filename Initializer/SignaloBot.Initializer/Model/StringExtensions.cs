using Common.EntityFramework;
using Common.MongoDb;
using SignaloBot.DAL.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Initializer
{
    public static class StringExtensions
    {
        public static string ToDetailsString(this MongoDbConnectionSettings settings)
        {
            return string.Format("MongoDb host:{0} port:{1} db:{2}"
                , settings.Host, settings.Port, settings.DatabaseName);
        }

        public static string ToDetailsString(this SqlConnetionSettings settings)
        {
            return string.Format("Sql connection string:{0} prefix:{1}"
                , settings.NameOrConnectionString, settings.Prefix);
        }

        
    }
}
