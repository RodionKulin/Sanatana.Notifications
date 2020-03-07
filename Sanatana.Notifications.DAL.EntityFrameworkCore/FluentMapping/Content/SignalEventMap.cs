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
            builder.Ignore(t => t.DataKeyValues);
            builder.Property(r => r.DataKeyValuesSerialized).HasColumnType("xml").HasColumnName("DataKeyValues");

            builder.Ignore(t => t.SubscriberIdFromDeliveryTypesHandled);
            builder.Property(r => r.SubscriberIdFromDeliveryTypesHandledSerialized).HasColumnType("xml").HasColumnName("SubscriberIdFromDeliveryTypesHandled");

            builder.Ignore(t => t.PredefinedAddresses);
            builder.Property(t => t.PredefinedAddressesSerialized).HasColumnName("PredefinedAddresses");

            builder.Ignore(t => t.PredefinedSubscriberIds);
            builder.Property(t => t.PredefinedSubscriberIdsSerialized).HasColumnName("PredefinedSubscriberIds");

            // Table
            builder.ToTable(DefaultTableNameConstants.SignalEvents, _connectionSettings.Schema);
        }
    }
}
