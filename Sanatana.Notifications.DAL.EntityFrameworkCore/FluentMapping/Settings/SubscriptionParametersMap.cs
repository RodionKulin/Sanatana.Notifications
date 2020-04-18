using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SubscriptionParametersMap : IEntityTypeConfiguration<SubscriptionParameters>
    {
        public void Configure(EntityTypeBuilder<SubscriptionParameters> builder)
        {
            builder.Ignore(t => t.SubscriberFiltersData);
        }
    }
}
