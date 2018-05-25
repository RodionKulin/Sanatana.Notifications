using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.EntityFrameworkCore.DI.Autofac;
using Sanatana.Notifications.DeliveryTypes.Trace;
using Sanatana.Notifications.DI.Autofac;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.NotificationsTests.TestTools
{
    public static class Dependencies
    {
        //fields
        private static IContainer _container;



        //properties
        public static IContainer Container
        {
            get
            {
                if (_container == null)
                {
                    _container = Configure();
                }

                return _container;
            }
        }



        //methods
        private static IContainer Configure()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new NotificationsAutofacModule<long>());
            builder.RegisterModule(new InMemoryComposerSettingsAutofacModule<long>(new List<ComposerSettings<long>>()));               
            builder.RegisterModule(new EntityFrameworkCoreAutofacModule(new EntityFrameworkCore.SqlConnectionSettings("", "")));
            builder.RegisterInstance(new DispatchChannel<long>()
            {
                Dispatcher = new TraceDispatcher<long>(),
                DeliveryType = (int)TestDeliveryTypes.Trace
            }).AsSelf().SingleInstance();
            builder.RegisterType<TraceMonitor<long>>().As<IMonitor<long>>().SingleInstance();

            ILogger logger = new ConsoleLogger("TestLogger", (input, level) => true, true);
            builder.RegisterInstance(logger).As<ILogger>().SingleInstance();

            IContainer container = builder.Build();
            return container;
        }

        public static T Resolve<T>()
        {
            IContainer contaier = Container;
            return contaier.Resolve<T>();
        }
    }
}
