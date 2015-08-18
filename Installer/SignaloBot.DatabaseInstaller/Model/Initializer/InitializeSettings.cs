using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DatabaseInstaller.Model.Initializer
{
    public class InitializeSettings
    {
        //connectinos string
        public string ConnectionString { get; set; }
        public string SqlPrefix { get; set; }

        //Install type
        public bool DropExistingDB { get; set; }

        //Sql part
        public bool InstallUserSettings { get; set; }
        public bool InstallSendQueue { get; set; }
        public bool InstallNDR { get; set; }
        public bool InstallNotifications { get; set; }

        //Demo data
        public bool InsertDemoData { get; set; }



        //методы
        public void PrintSettingsToConsole()
        {
            //полключение
            Console.WriteLine("Будет выполнена установка базы данных по подключению:");
            Console.WriteLine(ConnectionString);

            Console.WriteLine("Префикс sql таблиц: {0}", SqlPrefix);

            if (DropExistingDB)
            {
                Console.WriteLine("Существующая база данных будет удалена.");
            }
            

            //какие таблицы и скрипты устанавливать
            if (InstallUserSettings)
            {
                Console.WriteLine("Будут установлены таблицы и скрипты настроек пользователей.");
            }

            if (InstallSendQueue)
            {
                Console.WriteLine("Будет установлена таблица и скрипты очереди сообщений.");
            }

            if (InstallNDR)
            {
                Console.WriteLine("Будут установлены таблицы и скрипты NDR.");
            }

            if (InstallNotifications)
            {
                Console.WriteLine("Будут установлены таблицы и скрипты Notifications.");
            }



            //демо данные
            if (InsertDemoData)
            {
                Console.WriteLine("Будут добавлены тестовые данные.");
            }
        }
    }
}
