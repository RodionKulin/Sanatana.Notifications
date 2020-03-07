using Microsoft.EntityFrameworkCore;
using Moq;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Interfaces;
using SpecsFor.Core.Configuration;
using StructureMap.AutoMocking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCoreSpecs.TestTools.Providers
{
    public class DatabaseCreator : Behavior<INeedDbContext>
    {
        //fields
        private static bool _isInitialized;


        //methods
        public override void SpecInit(INeedDbContext instance)
        {
            if (_isInitialized)
            {
                return;
            }

            dynamic specsDynamic = instance;
            AutoMockedContainer autoMockContainer = specsDynamic.Mocker.MoqAutoMocker.Container;
            ISenderDbContextFactory factory = autoMockContainer.GetInstance<ISenderDbContextFactory>();
            factory.InitializeDatabase();

            _isInitialized = true;
        }
    }
}
