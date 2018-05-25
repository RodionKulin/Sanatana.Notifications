using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Sanatana.Notifications.Demo.Sender.Model
{
    public class CustomDbContext : SenderDbContext
    {
        //init
        public CustomDbContext(SqlConnectionSettings connectionSettings)
            : base(connectionSettings)
        {
        }
        

        //methods
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //modelBuilder.Ignore<ComposerSettingsLong>();
            //modelBuilder.Ignore<DispatchTemplateLong>();
        }
    }
}
