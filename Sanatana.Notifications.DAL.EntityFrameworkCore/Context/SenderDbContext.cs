using Sanatana.EntityFrameworkCore;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.Parameters;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.Context
{
    public class SenderDbContext : DbContext
    {
        //fields
        protected SqlConnectionSettings _connectionSettings;


        //tables
        public DbSet<ComposerSettingsLong> ComposerSettings { get; set; }
        public DbSet<DispatchTemplateLong> DispatchTemplates { get; set; }
        public DbSet<SignalBounceLong> SignalBounces { get; set; }
        public DbSet<SignalEventLong> SignalEvents { get; set; }
        public DbSet<SignalDispatchLong> SignalDispatches { get; set; }
        public DbSet<StoredNotificationLong> StoredNotifications { get; set; }
        public DbSet<SubscriberDeliveryTypeSettingsLong> SubscriberDeliveryTypeSettings { get; set; }
        public DbSet<SubscriberCategorySettingsLong> SubscriberCategorySettings { get; set; }
        public DbSet<SubscriberTopicSettingsLong> SubscriberTopicSettings { get; set; }
        public DbSet<SubscriberScheduleSettingsLong> SubscriberScheduleSettings { get; set; }


        //init
        public SenderDbContext(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
               .UseSqlServer(_connectionSettings.ConnectionString, options => options.CommandTimeout(30));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ComposerSettings<long>>();
            modelBuilder.Ignore<DispatchTemplate<long>>();
            modelBuilder.Ignore<SignalBounce<long>>();
            modelBuilder.Ignore<SignalEvent<long>>();
            modelBuilder.Ignore<SignalDispatch<long>>();
            modelBuilder.Ignore<StoredNotification<long>>();
            modelBuilder.Ignore<SubscriberDeliveryTypeSettings<long>>();
            modelBuilder.Ignore<SubscriberCategorySettings<long>>();
            modelBuilder.Ignore<SubscriberTopicSettings<long>>();
            modelBuilder.Ignore<SubscriberScheduleSettings<long>>();

            modelBuilder.ApplyConfiguration(new ComposerSettingsMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new DispatchTemplateMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SignalBounceMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SignalEventMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SignalDispatchMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new StoredNotificationMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SubscriberDeliveryTypeSettingsMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SubscriberCategorySettingsMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SubscriberTopicSettingsMap(_connectionSettings));
            modelBuilder.ApplyConfiguration(new SubscriberScheduleSettingsMap(_connectionSettings));

            modelBuilder.Ignore<DeliveryAddress>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
