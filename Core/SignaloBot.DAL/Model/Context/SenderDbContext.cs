using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Entities.CoreMapping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Context
{
    public class SenderDbContext : DbContext
    {
        //поля
        protected string _prefix;

        //свойства
        public DbSet<Signal> Signals { get; set; }


        //инициализация
        public SenderDbContext(string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            //по умолчанию отменяет проверку совместимости модели
            System.Data.Entity.Database.SetInitializer<SenderDbContext>(null);
        }

        public SenderDbContext(IDatabaseInitializer<SenderDbContext> initializer
            , string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            System.Data.Entity.Database.SetInitializer<SenderDbContext>(initializer);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SignalMap(_prefix));

            base.OnModelCreating(modelBuilder);
        }
    }
}
