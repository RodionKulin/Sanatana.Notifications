using Common.TestUtility;
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
    public static class AmazonTestParameters
    {
        //свойства
        public static string SenderEmail { get; set; }
        public static string ReceiverEmail { get; set; }
        public static string AWSAccessKey { get; set; }
        public static string AWSSecretKey { get; set; }
        public static string AWSRegion { get; set; }


        //инициализация
        static AmazonTestParameters()
        {
            ConfigParametersLoader loader = ParametersUtility.GetConfigLoader();

            SenderEmail = loader.ReadAppSettingsValue("AmazonSenderEmail");
            ReceiverEmail = loader.ReadAppSettingsValue("AmazonReceiverEmail");
            AWSAccessKey = loader.ReadAppSettingsValue("AWSAccessKey");
            AWSSecretKey = loader.ReadAppSettingsValue("AWSSecretKey");
            AWSRegion = loader.ReadAppSettingsValue("AWSRegion");            
        }
    }
}
