using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class ComposerSettingsMap : IEntityTypeConfiguration<ComposerSettingsLong>
    {
        protected SqlConnectionSettings _connectionSettings;

        public ComposerSettingsMap(SqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Configure(EntityTypeBuilder<ComposerSettingsLong> builder)
        {
            // Keys
            builder.HasKey(t => t.ComposerSettingsId);


            // Properties
            builder.OwnsOne(p => p.Subscription, build => build
                .Ignore(x => x.SelectFromCategories)
                .Ignore(x => x.SelectFromTopics));
            builder.OwnsOne(p => p.Updates, build => build
                .Ignore(x => x.UpdateAnything)
                .Ignore(x => x.UpdateDeliveryType)
                .Ignore(x => x.UpdateCategory)
                .Ignore(x => x.UpdateTopic));
            builder.Ignore(p => p.Templates);


            // Table
            builder.ToTable(DefaultTableNameConstants.ComposerSettings, _connectionSettings.Schema);
        }
    }
}
