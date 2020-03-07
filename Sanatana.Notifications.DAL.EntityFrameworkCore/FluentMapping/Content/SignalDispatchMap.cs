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
    public class SignalDispatchMap : IEntityTypeConfiguration<SignalDispatchLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SignalDispatchMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<SignalDispatchLong> builder)
        {
            // Keys
            builder.HasKey(t => t.SignalDispatchId);
            builder.HasIndex(t => new { t.SendDateUtc, t.FailedAttempts }).IsUnique(false);
            builder.HasIndex(t => new { t.ReceiverSubscriberId, t.SendDateUtc }).IsUnique(false);
            
            // Properties
            builder.Property(t => t.ReceiverAddress).IsRequired();
            builder.Property(t => t.SendDateUtc).HasColumnType("datetime2");
            builder.Property(t => t.CreateDateUtc).HasColumnType("datetime2");
            
            // Table 
            builder.ToTable(DefaultTableNameConstants.SignalDispatches, _connectionSettings.Schema);
        }
    }
}
