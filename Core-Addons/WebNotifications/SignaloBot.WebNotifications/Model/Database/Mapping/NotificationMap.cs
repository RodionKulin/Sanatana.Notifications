using SignaloBot.WebNotifications.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.WebNotifications.Database.Mapping
{
    internal class NotificationMap : EntityTypeConfiguration<Notification>
    {
        public NotificationMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => t.NotificationID);


            // Properties
            this.Property(t => t.TopicID).IsRequired().HasMaxLength(4000);
            this.Property(t => t.NotifyText).IsRequired();
            this.Property(t => t.SendDateUtc).HasColumnType("datetime2");
            this.Property(t => t.Culture).IsRequired();
                        

            // Table & Column Mappings
            this.ToTable(prefix + "Notifications");
          
        }
    }
}
