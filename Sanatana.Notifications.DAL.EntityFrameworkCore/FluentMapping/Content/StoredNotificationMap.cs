using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class StoredNotificationMap : IEntityTypeConfiguration<StoredNotificationLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public StoredNotificationMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<StoredNotificationLong> builder)
        {
            // Key
            builder.HasKey(t => t.StoredNotificationId);
            builder.HasIndex(t => new { t.SubscriberId, t.CreateDateUtc }).IsUnique(false);
            
            // Properties
            builder.Property(t => t.CreateDateUtc).HasColumnType("datetime2");

            // Table 
            builder.ToTable(DefaultTableNameConstants.StoredNotifications, _connectionSettings.Schema);
        }
    }
}
