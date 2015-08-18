using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.DAL.Entities.CoreMapping
{
    internal class UserCategorySettingsMap : EntityTypeConfiguration<UserCategorySettings>
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
