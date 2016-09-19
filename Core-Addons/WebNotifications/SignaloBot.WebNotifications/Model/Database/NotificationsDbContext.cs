using SignaloBot.WebNotifications.Database.Mapping;
using SignaloBot.WebNotifications.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.WebNotifications.Database
{
    public class NotificationsDbContext : DbContext
    {
        //поля
        protected string _prefix;

        //свойства
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationMeta> NotificationsMeta { get; set; }
      



        //инициализация
        public NotificationsDbContext(string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            //по умолчанию отменяет проверку совместимости модели
            System.Data.Entity.Database.SetInitializer<NotificationsDbContext>(null);
        }

        public NotificationsDbContext(IDatabaseInitializer<NotificationsDbContext> initializer,
            string nameOrConnectionString, string prefix = null)
            : base(nameOrConnectionString)
        {
            _prefix = prefix;

            //по умолчанию отменяет проверку совместимости модели
            System.Data.Entity.Database.SetInitializer<NotificationsDbContext>(initializer);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new NotificationMap(_prefix));
            modelBuilder.Configurations.Add(new NotificationMetaMap(_prefix));
           
            base.OnModelCreating(modelBuilder);
        }
    }
}
