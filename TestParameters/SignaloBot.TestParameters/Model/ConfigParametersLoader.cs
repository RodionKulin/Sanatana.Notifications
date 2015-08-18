using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AlarmoBot.TestParameters.Model
{
    public class ConfigParametersLoader
    {
        //поля
        public string ExampleConfig { get; set; }


        //инициализация
        public ConfigParametersLoader()
        {
        }
        public ConfigParametersLoader(string exampleConfig)
        {
            ExampleConfig = exampleConfig;
        }
        public static ConfigParametersLoader FromResource(string exampleConfigResourceName)
        {
            ConfigParametersLoader configParametersLoader = new ConfigParametersLoader();
            configParametersLoader.ExampleConfig = GetExampleConfig(exampleConfigResourceName);
            return configParametersLoader;
        }

        
        //методы
        public string ReadAppSettingsValue(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if(string.IsNullOrEmpty(value))
            {
                string assemblyName = Assembly.GetCallingAssembly().FullName;
                string message = string.Format("Параметр appSettings {0} не установлен в конфиге сборки {1}.", key, assemblyName);
                ThrowConfigException(message);
            }

            return value;
        }

        public string ReadConnectionString(string key)
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[key];
            
            if (connectionString == null
                || string.IsNullOrEmpty(connectionString.ConnectionString))
            {
                string assemblyName = Assembly.GetCallingAssembly().FullName;
                string message = string.Format("Строка подключения {0} не найдена в конфиге сборки {1}.", key, assemblyName);
                ThrowConfigException(message);
            }

            return connectionString.ConnectionString;
        }

        public void ThrowConfigException(string message)
        {
            if (!string.IsNullOrEmpty(ExampleConfig))
            {
                message = string.Format(@"{0}

Пример содержания конфига:
{1}"
               , message, ExampleConfig);

            }

            throw new KeyNotFoundException(message);
        }

        private static string GetExampleConfig(string resourceName)
        {
            string resourceContent = null;

            Assembly assembly = Assembly.GetCallingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                resourceContent = reader.ReadToEnd();
            }

            return resourceContent;
        }

    }
}
