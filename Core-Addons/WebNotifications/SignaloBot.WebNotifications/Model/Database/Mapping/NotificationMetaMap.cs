using SignaloBot.WebNotifications.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.WebNotifications.Database.Mapping
{
    internal class NotificationMetaMap : EntityTypeConfiguration<NotificationMeta>
    {
        public NotificationMetaMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => t.NotificationMetaID);

            // Properties
            this.Property(t => t.TopicID)
                .IsRequired().HasMaxLength(4000);

            this.Property(t => t.MetaType)
                .IsRequired().HasMaxLength(4000);

            this.Property(t => t.MetaKey)
                .IsRequired().HasMaxLength(4000);

            this.Property(t => t.MetaValue)
                .IsRequired().HasMaxLength(4000);


            // Table & Column Mappings
            this.ToTable(prefix + "NotificationsMeta");
          
        }
    }
}
