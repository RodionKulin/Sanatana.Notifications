using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using SpecsFor.Core.Configuration;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DataPurger : Behavior<INeedDbContext>
    {
        public override void SpecInit(INeedDbContext instance)
        {
            SenderDbContext context = instance.DbContext;

            //Disable all foreign keys.
            context.Database.ExecuteSqlRaw("EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"");

            //Remove all data from tables EXCEPT for the EF Migration History table!
            context.Database.ExecuteSqlRaw("EXEC sp_msforeachtable \"SET QUOTED_IDENTIFIER ON; IF '?' != '[dbo].[__MigrationHistory]' DELETE FROM ?\"");

            //Turn FKs back on
            context.Database.ExecuteSqlRaw("EXEC sp_msforeachtable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all\"");

        }
    }
}
