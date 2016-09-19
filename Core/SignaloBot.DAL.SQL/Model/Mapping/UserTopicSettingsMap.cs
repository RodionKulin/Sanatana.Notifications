using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.DAL.SQL
{
    internal class UserTopicSettingsMap : EntityTypeConfiguration<UserTopicSettingsGuid>
    {
        public UserTopicSettingsMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => new { t.CategoryID, t.TopicID });

            // Properties
            this.Property(t => t.TopicID).IsRequired().HasMaxLength(1000);
            this.Property(t => t.AddDateUtc).HasColumnType("datetime2");
            this.Property(t => t.LastSendDateUtc).HasColumnType("datetime2");


            // Table & Column Mappings
            this.ToTable(prefix + "UserTopicSettings");
        }
    }
}
