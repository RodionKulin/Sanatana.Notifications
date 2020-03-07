using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SubscriberDeliveryTypeSettingsMap : IEntityTypeConfiguration<SubscriberDeliveryTypeSettingsLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SubscriberDeliveryTypeSettingsMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<SubscriberDeliveryTypeSettingsLong> builder)
        {
            // Primary Key
            builder.HasKey(t => t.SubscriberDeliveryTypeSettingsId);
            builder.HasIndex(t => t.SubscriberId).IsUnique(false);
            builder.HasIndex(t => t.Address).IsUnique(false);
            builder.HasIndex(t => t.GroupId).IsUnique(false);
            
            // Properties
            builder.Property(t => t.Address).IsRequired().HasMaxLength(1000);
            builder.Property(t => t.LastVisitUtc).HasColumnType("datetime2");
            builder.Property(t => t.LastSendDateUtc).HasColumnType("datetime2");
            builder.Property(t => t.NDRBlockResetCodeSendDateUtc).HasColumnType("datetime2");

            //Ignore
            builder.Ignore(t => t.SubscriberCategorySettings);

            // Table 
            builder.ToTable(DefaultTableNameConstants.SubscriberDeliveryTypeSettings, _connectionSettings.Schema);
        }
    }
}
