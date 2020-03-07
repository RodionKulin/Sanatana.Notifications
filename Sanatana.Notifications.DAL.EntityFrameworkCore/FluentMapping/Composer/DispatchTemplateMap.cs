using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class DispatchTemplateMap : IEntityTypeConfiguration<DispatchTemplateLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public DispatchTemplateMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }


        public void Configure(EntityTypeBuilder<DispatchTemplateLong> builder)
        {
            // Keys
            builder.HasKey(t => t.DispatchTemplateId);

            // Properties
            builder.HasOne(x => x.EventSettingsNavigation)
                .WithMany(x => x.TemplatesNavigation)
                .HasForeignKey(x => x.EventSettingsId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table
            builder.ToTable(DefaultTableNameConstants.DispatchTemplates, _connectionSettings.Schema);
        }
    }
}
