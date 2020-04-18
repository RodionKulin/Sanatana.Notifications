using Sanatana.Notifications.DAL.MongoDbSpecs.SpecObjects;
using Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDbSpecs.TestTools.Behaviors
{
    public class DbContextProvider : Behavior<INeedDbContext>
    {
        public override void SpecInit(INeedDbContext instance)
        {
            instance.DbContext = instance.Mocker.GetServiceInstance<SpecsDbContext>();
        }
    }
}
