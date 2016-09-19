using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    public class SenderDbContext : DbContext
    {
        //поля
        protected string _prefix;

        //свойства
        public DbSet<SignalDispatchBaseGuid> SignalsDispatches { get; set; }
        public DbSet<SubjectDispatchGuid> SubjectDispatches { get; set; }


        //инициализация
        public SenderDbContext(string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            Configuration.ProxyCreationEnabled = false;
            //по умолчанию отменяет проверку совместимости модели
            System.Data.Entity.Database.SetInitializer<SenderDbContext>(null);
        }

        public SenderDbContext(IDatabaseInitializer<SenderDbContext> initializer
            , string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            Configuration.ProxyCreationEnabled = false;
            System.Data.Entity.Database.SetInitializer<SenderDbContext>(initializer);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SignalDispatchBaseMap(_prefix));
            modelBuilder.Configurations.Add(new SubjectDispatchMap(_prefix));
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
