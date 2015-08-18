using SignaloBot.DAL.Entities.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.CoreMapping
{
    internal class UserDeliveryTypeSettingsMap : EntityTypeConfiguration<UserDeliveryTypeSettings>
    {
        public UserDeliveryTypeSettingsMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => new { t.UserID, t.DeliveryType });

            // Properties
            this.Property(t => t.Address).IsRequired().HasMaxLength(1000);
            this.Property(t => t.LastUserVisitUtc).HasColumnType("datetime2");
            this.Property(t => t.LastSendDateUtc).HasColumnType("datetime2");
            this.Property(t => t.BlockOfNDRResetCodeSendDateUtc).HasColumnType("datetime2");

            // Table & Column Mappings
            this.ToTable(prefix + "UserDeliveryTypeSettings");
          
        }
    }
}
