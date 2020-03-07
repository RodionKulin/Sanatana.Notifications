using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.EntityFrameworkCore.Batch;

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
            
            //modelBuilder.Ignore<EventSettingsLong>();
            //modelBuilder.Ignore<DispatchTemplateLong>();
        }
    }
}
