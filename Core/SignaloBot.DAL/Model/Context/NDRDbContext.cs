using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.CoreMapping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Context
{
    public class NDRDbContext : DbContext
    {
        //поля
        protected string _prefix;

        //свойства
        public DbSet<BouncedMessage> BouncedMessages { get; set; }


        
        //инициализация
        public NDRDbContext(string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            //по умолчанию отменяет проверку совместимости модели
            System.Data.Entity.Database.SetInitializer<NDRDbContext>(null);
        }

        public NDRDbContext(IDatabaseInitializer<NDRDbContext> initializer
            , string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            System.Data.Entity.Database.SetInitializer<NDRDbContext>(initializer);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new BouncedMessageMap(_prefix));
          
            base.OnModelCreating(modelBuilder);
        }
    }
}
