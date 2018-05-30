using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SubscriberCategorySettingsMap : IEntityTypeConfiguration<SubscriberCategorySettingsLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SubscriberCategorySettingsMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<SubscriberCategorySettingsLong> builder)
        {
            // Key
            builder.HasKey(t => t.SubscriberCategorySettingsId);
            builder.HasIndex(t => t.SubscriberId).IsUnique(false);
            builder.HasIndex(t => new { t.GroupId, t.CategoryId }).IsUnique(false);
            
            // Properties
            builder.Property(t => t.LastSendDateUtc).HasColumnType("datetime2");
            
            // Table 
            builder.ToTable(DefaultTableNameConstants.SubscriberCategorySettings, _connectionSettings.Schema);
        }
    }
}
