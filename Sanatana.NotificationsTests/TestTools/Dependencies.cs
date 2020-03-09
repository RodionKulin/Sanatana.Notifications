using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.EntityFrameworkCore.DI.Autofac;
using Sanatana.Notifications.DeliveryTypes.Trace;
using Sanatana.Notifications.DI.Autofac;
using Sanatana.Notifications.Dispatching.Channels;
using Sanatana.Notifications.EventTracking;
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
            builder.RegisterModule(new NotificationsCoreAutofacModule<long>());
            builder.RegisterModule(new InMemoryEventSettingsAutofacModule<long>(new List<EventSettings<long>>()));               
            builder.RegisterModule(new EntityFrameworkCoreSenderAutofacModule(new SqlConnectionSettings("", "")));
            builder.RegisterInstance(new DispatchChannel<long>()
            {
                Dispatcher = new TraceDispatcher<long>(),
                DeliveryType = (int)TestDeliveryTypes.Trace
            }).AsSelf().SingleInstance();
            builder.RegisterType<TraceEventTracker<long>>().As<IEventTracker<long>>().SingleInstance();

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
