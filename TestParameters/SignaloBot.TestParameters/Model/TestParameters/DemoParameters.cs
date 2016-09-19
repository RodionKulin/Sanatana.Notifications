using Common.TestUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.TestParameters.Model.TestParameters
{
    public static class DemoParameters
    {  
        //свойства
        public static string EmailFromAddress { get; set; }

        public static string SmtpServer { get; set; }
        public static int Port { get; set; }
        public static bool EnableSsl { get; set; }
        public static string Login { get; set; }
        public static string Password { get; set; }



        //инициализация
        static DemoParameters()
        {
            ConfigParametersLoader loader = ParametersUtility.GetConfigLoader();

            EmailFromAddress = loader.ReadAppSettingsValue("EmailFromAddress");

            SmtpServer = loader.ReadAppSettingsValue("SmtpServer");
            Port = loader.ReadAppSettingsValue<int>("Port");
            EnableSsl = loader.ReadAppSettingsValue<bool>("EnableSsl");
            Login = loader.ReadAppSettingsValue("Login");
            Password = loader.ReadAppSettingsValue("Password");
        }

    }
}
