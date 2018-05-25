using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SignalBounceMap : IEntityTypeConfiguration<SignalBounceLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public SignalBounceMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<SignalBounceLong> builder)
        {
            // Key
            builder.HasKey(t => t.SignalBounceId);
            builder.HasIndex(t => new { t.ReceiverSubscriberId, t.BounceReceiveDateUtc }).IsUnique(false);
            
            // Properties
            builder.Property(t => t.ReceiverAddress).IsRequired();
            builder.Property(t => t.BounceReceiveDateUtc).HasColumnType("datetime2");
            builder.Property(t => t.SendDateUtc).HasColumnType("datetime2");
            builder.Property(t => t.BounceDetailsXML).HasColumnType("xml");
            
            // Table
            builder.ToTable(DefaultTableNameConstants.SignalBounces, _connectionSettings.Schema);
        }
    }
}
