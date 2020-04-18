using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SubscriberTopicSettingsMap : IEntityTypeConfiguration<SubscriberTopicSettingsLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SubscriberTopicSettingsMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<SubscriberTopicSettingsLong> builder)
        {
            // Keys
            builder.HasKey(t => t.SubscriberTopicSettingsId);
            builder.HasIndex(t => t.SubscriberId).IsUnique(false);
            builder.HasIndex(t => t.TopicId).IsUnique(false);
            
            // Properties
            builder.Property(t => t.TopicId).IsRequired().HasMaxLength(1000);
            builder.Property(t => t.AddDateUtc).HasColumnType("datetime2");
            builder.Property(t => t.LastSendDateUtc).HasColumnType("datetime2");
            
            // Table 
            builder.ToTable(DefaultTableNameConstants.SubscriberTopicSettings, _connectionSettings.Schema);
        }
    }
}
