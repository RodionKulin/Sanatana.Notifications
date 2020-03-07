using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SubscriberScheduleSettingsMap : IEntityTypeConfiguration<SubscriberScheduleSettingsLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SubscriberScheduleSettingsMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<SubscriberScheduleSettingsLong> builder)
        {
            // Keys
            builder.HasKey(t => t.SubscriberScheduleSettingsId);
            builder.HasIndex(t => t.SubscriberId).IsUnique(false);
            
            // Table
            builder.ToTable(DefaultTableNameConstants.SubscriberScheduleSettings, _connectionSettings.Schema);
        }
    }
}
