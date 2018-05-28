using MongoDB.Bson;
using Sanatana.Notifications;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.Dispatching.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Sanatana.Notifications.DAL.EntityFrameworkCore;
using Sanatana.Notifications.DAL.EntityFrameworkCore.Context;
using Sanatana.Notifications.Demo.Sender.Model;
using Sanatana.Notifications.Monitoring;
using Sanatana.Notifications.DeliveryTypes.Email;
using Sanatana.Notifications.DeliveryTypes.Trace;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Parameters;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.DI.Autofac;
using Sanatana.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.SignalProviders.WCF.DI.Autofac;
using Sanatana.Notifications.DAL.EntityFrameworkCore.DI.Autofac;

namespace Sanatana.Notifications.Demo.Sender
{
    class Program
    {
        //methods
        static void Main(string[] args)
        {
            ISender sender = CreateSenderWithMsSql();
            sender.Start();
            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();
            sender.Stop(true);
        }

        private static ISender CreateSenderWithMsSql()
        {
            bool initDatabase = true;
            SqlConnectionSettings connection = new SqlConnectionSettings
            {
                Schema = "msg",
                ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new NotificationsAutofacModule<long>());
            builder.RegisterModule(new InMemoryEventSettingsAutofacModule<long>(GetEventSettings()));
            //builder.RegisterModule(new EntityFrameworkEventSettingsAutofacModule(useCaching: true));
            builder.RegisterModule(new EntityFrameworkCoreAutofacModule(connection));
            builder.RegisterInstance(new DispatchChannel<long>()
            {
                Dispatcher = new ConsoleDispatcher<long>(),
                DeliveryType = (int)Model.DeliveryTypes.Console
            }).AsSelf().SingleInstance();
            builder.RegisterType<StoredNotificationsDispatchChannel<long>>().As<DispatchChannel<long>>().SingleInstance();
            builder.RegisterModule(new WcfSignalEndpointAutofacModule<long>());
            ILogger logger = new ConsoleLogger("DemoLogger", (input, level) => true, true);
            builder.RegisterInstance(logger).As<ILogger>().SingleInstance();

            IContainer container = builder.Build();
            if (initDatabase)
            {
                ISenderDbContextFactory contextFactory = container.Resolve<ISenderDbContextFactory>();
                contextFactory.InitializeDatabase();
            }

            ISender sender = container.Resolve<ISender>();
            return sender;

        }

        private static List<EventSettings<long>> GetEventSettings()
        {
            return new List<EventSettings<long>>()
            {
                new EventSettings<long>()
                {
                    CategoryId = (int)CategoryTypes.CustomerGreetings,
                    EventSettingsId = 1,
                    Subscription = new SubscriptionParameters()
                    {
                        CategoryId = (int)CategoryTypes.CustomerGreetings
                    },
                    CompositionHandlerId = null,
                    Templates = new List<DispatchTemplate<long>>()
                    {
                        new EmailDispatchTemplate<long>()
                        {
                            DeliveryType = (int)Model.DeliveryTypes.Email,
                            IsBodyHtml = false,
                            SubjectProvider = (StringTemplate)"Test subject to {customer}",
                            SubjectTransformer = new ReplaceTransformer(),
                            BodyProvider = (StringTemplate)"Hello {customer}",
                            BodyTransformer = new ReplaceTransformer(),
                        }
                    },
                    Updates = new UpdateParameters()
                }
            };
        }
        
    }

}
