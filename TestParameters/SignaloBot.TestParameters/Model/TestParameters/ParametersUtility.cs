using Common.TestUtility.Config;
using Common.TestUtility.Model.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.TestParameters.Model
{
    internal class ParametersUtility
    {
        public static string GetConfigPath()
        {
            string sameDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.CONFIG_FILE_NAME);
            string wwwPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", Constants.CONFIG_FILE_NAME);

            if (File.Exists(sameDirPath))
            {
                return sameDirPath;
            }
            else if (File.Exists(wwwPath))
            {
                return wwwPath;
            }
            else
            {
                return sameDirPath;
            }
        }

        public static ConfigParametersLoader GetConfigLoader()
        {
            Assembly configResourceAssembly = Assembly.GetCallingAssembly();
            string exampleConfig = ResourceHelper.ReadResource(configResourceAssembly, Constants.EXAMPLE_CONFIG_RESOURCE);
            string configPath = GetConfigPath();
            ConfigParametersLoader loader = new ConfigParametersLoader(configPath, exampleConfig);

            return loader;
        }
    }
}
