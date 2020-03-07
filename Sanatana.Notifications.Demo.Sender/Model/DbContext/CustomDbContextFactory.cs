using Sanatana.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Demo.Sender.Model
{
    public class CustomDbContextFactory : SenderDbContextFactory
    {

        //init
        public CustomDbContextFactory(SqlConnectionSettings connectionSettings)
            : base(connectionSettings)
        {

        }


        //methods
        public override SenderDbContext GetDbContext()
        {
            
            return new CustomDbContext(_connectionSettings);
        }
    }
}
