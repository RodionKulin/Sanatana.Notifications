using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.SQL
{
    public class ClientDbContext : DbContext
    {
        //поля
        protected string _prefix;

        //свойства
        public DbSet<UserDeliveryTypeSettingsGuid> UserDeliveryTypeSettings { get; set; }
        public DbSet<UserCategorySettingsGuid> UserCategorySettings { get; set; }
        public DbSet<UserTopicSettingsGuid> UserTopicSettings { get; set; }
        public DbSet<UserReceivePeriodGuid> UserReceivePeriods { get; set; }



        //инициализация
        public ClientDbContext(string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            //по умолчанию отменяет проверку совместимости модели
            System.Data.Entity.Database.SetInitializer<ClientDbContext>(null);
        }

        public ClientDbContext(IDatabaseInitializer<ClientDbContext> initializer
            , string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            System.Data.Entity.Database.SetInitializer<ClientDbContext>(initializer);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new UserDeliveryTypeSettingsMap(_prefix));
            modelBuilder.Configurations.Add(new UserCategorySettingsMap(_prefix));
            modelBuilder.Configurations.Add(new UserTopicSettingsMap(_prefix));
            modelBuilder.Configurations.Add(new UserReceivePeriodMap(_prefix));

            base.OnModelCreating(modelBuilder);
        }
    }
}
