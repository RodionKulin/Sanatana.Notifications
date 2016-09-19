using Common.EntityFramework;
using Common.Initializer;
using SignaloBot.DAL.SQL;
using SignaloBot.Initializer.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Initializer
{
    public class DropSqlDbModule : IInitializeModule
    {
        //поля
        private SqlConnetionSettings _connection;


        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public DropSqlDbModule(SqlConnetionSettings connection)
        {
            _connection = connection;
        }


        //методы
        public string IntroduceSelf()
        {
            return InnerMessages.DropSqlDb_Intro;
        }

        public Task Execute()
        {
            using (ClientDbContext context = new ClientDbContext(
                _connection.NameOrConnectionString, _connection.Prefix))
            {
                context.Database.Delete();
            }

            return Task.FromResult(true);
        }

    }
}
