using Common.TestUtility.Config;
using Common.TestUtility.Model.Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.TestParameters.Model
{
    public static class SignaloBotTestParameters
    {
        //свойства
        public static string ConnectionString { get; set; }
        public static string SqlPrefix { get; set; }

        public static Guid ExistingUserID { get; set; }
        public static int ExistingDeliveryType { get; set; }
        public static int ExistingCategoryID { get; set; }
        public static int ExistingSubscriptionTopicID { get; set; }


        //инициализация
        static SignaloBotTestParameters()
        {
            ConfigParametersLoader loader = ParametersUtility.GetConfigLoader();

            //connection string
            ConnectionString = loader.ReadConnectionString("DefaultConnection");

            //app settings
            SqlPrefix = loader.ReadAppSettingsValue("SqlPrefix");

            //SignaloBot.Client.Tests common parameters
            ExistingUserID = new Guid("A3D44032-40F8-44C0-89C4-FB9393B81FCE");
            ExistingDeliveryType = 1;
            ExistingCategoryID = 1;
            ExistingSubscriptionTopicID = 1;
        }
    }
}
