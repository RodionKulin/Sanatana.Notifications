using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.DAL.SQL
{
    internal class UserCategorySettingsMap : EntityTypeConfiguration<UserCategorySettingsGuid>
    {
        public UserCategorySettingsMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => new { t.UserID, t.DeliveryType, t.CategoryID });

            // Properties
            this.Property(t => t.LastSendDateUtc).HasColumnType("datetime2");


            // Table & Column Mappings
            this.ToTable(prefix + "UserCategorySettings");
          
        }
    }
}
