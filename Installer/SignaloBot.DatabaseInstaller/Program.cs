using SignaloBot.DatabaseInstaller.Initializer;
using SignaloBot.DatabaseInstaller.Model.Initializer;
using SignaloBot.TestParameters.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignaloBot.DatabaseInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Install();
        }

        private static void Install()
        {
            InitializeSettings settings = new InitializeSettings()
            {
                //подключение
                DropExistingDB = true,
                ConnectionString = SignaloBotTestParameters.ConnectionString,
                SqlPrefix = SignaloBotTestParameters.SqlPrefix,

                //какие таблицы и скрипты устанавливать
                InstallUserSettings = true,
                InstallSendQueue = true,
                InstallNotifications = true,
                InstallNDR = true,

                //демо данные
                InsertDemoData = true
            };

            settings.PrintSettingsToConsole();

            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для продолжения.");
            Console.ReadLine();
            Console.WriteLine("Установка базы данных начата.");
            Console.WriteLine();

            var initializer = new SignaloBotInitializer();
            initializer.RunScripts(settings);

            
            Console.WriteLine("Выполнено!");
            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для выхода.");
            Console.ReadLine();
        }

        
    }
}
