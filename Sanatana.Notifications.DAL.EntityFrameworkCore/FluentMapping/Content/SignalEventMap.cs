using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SignalEventMap : IEntityTypeConfiguration<SignalEventLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SignalEventMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<SignalEventLong> builder)
        {
            // Key
            builder.HasKey(t => t.SignalEventId);
            builder.HasIndex(t => new { t.FailedAttempts }).IsUnique(false);
            
            // Properties
            builder.Ignore(t => t.TemplateData);
            builder.Property(r => r.TemplateDataSerialized).HasColumnName("TemplateData").HasColumnType("nvarchar(max)");

            builder.Ignore(t => t.SubscriberFiltersData);
            builder.Property(r => r.SubscriberFiltersDataSerialized).HasColumnName("SubscriberFiltersData").HasColumnType("nvarchar(max)");

            builder.Ignore(t => t.SubscriberIdFromDeliveryTypesHandled);
            builder.Property(r => r.SubscriberIdFromDeliveryTypesHandledSerialized).HasColumnName("SubscriberIdFromDeliveryTypesHandled").HasColumnType("nvarchar(max)");

            builder.Ignore(t => t.PredefinedAddresses);
            builder.Property(t => t.PredefinedAddressesSerialized).HasColumnName("PredefinedAddresses").HasColumnType("nvarchar(max)");

            builder.Ignore(t => t.PredefinedSubscriberIds);
            builder.Property(t => t.PredefinedSubscriberIdsSerialized).HasColumnName("PredefinedSubscriberIds").HasColumnType("nvarchar(max)");

            // Table
            builder.ToTable(DefaultTableNameConstants.SignalEvents, _connectionSettings.Schema);
        }
    }
}
