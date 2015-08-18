using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SignaloBot.DAL.Entities.CoreMapping
{
    internal class UserReceivePeriodMap : EntityTypeConfiguration<UserReceivePeriod>
    {
        public UserReceivePeriodMap(string prefix)
        {
            // Primary Key
            this.HasKey(t => new { t.UserID, t.DeliveryType, t.CategoryID, t.PeriodOrder });

            // Properties
            this.Ignore(t => t.PeriodBeginString);
            this.Ignore(t => t.PeriodEndString);


            // Table & Column Mappings
            this.ToTable(prefix + "UserReceivePeriods");
          
        }
    }
}
