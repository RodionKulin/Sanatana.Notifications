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
        //fields
        private static long _nextComposerSettingsKey = 0;


        //methods
        static void Main(string[] args)
        {
            ISender sender = CreateSenderWithSQL();
            sender.Start();

            Console.WriteLine("Press enter to stop.");
            Console.ReadLine();

            sender.Stop(true);
        }

        private static ISender CreateSenderWithSQL()
        {
            bool initDatabase = true;
            SqlConnectionSettings connection = new SqlConnectionSettings
            {
                Schema = "msg",
                ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new NotificationsAutofacModule<long>());
            //builder.RegisterModule(new InMemoryComposerSettingsAutofacModule<long>(GetComposerSettings<long>()));
            builder.RegisterModule(new EntityFrameworkComposerSettingsAutofacModule(useCaching: true));
            builder.RegisterModule(new EntityFrameworkCoreAutofacModule(connection));
            builder.RegisterModule(new WcfSignalEndpointAutofacModule<long>());
            builder.RegisterInstance(new DispatchChannel<long>()
            {
                Dispatcher = new ConsoleDispatcher<long>(),
                DeliveryType = (int)Model.DeliveryTypes.Console
            }).AsSelf().SingleInstance();
            builder.RegisterType<StoredNotificationsDispatchChannel<long>>().As<DispatchChannel<long>>().SingleInstance();
            builder.RegisterType<CustomDbContextFactory>().As<ISenderDbContextFactory>().SingleInstance();
            builder.RegisterType<TraceMonitor<long>>().As<IMonitor<long>>().SingleInstance();

            ILogger logger = new ConsoleLogger("DemoLogger", (input, level) => true, true);
            builder.RegisterInstance(logger).As<ILogger>().SingleInstance();

            IContainer container = builder.Build();
            ISenderDbContextFactory contextFactory = container.Resolve<ISenderDbContextFactory>();
            if (initDatabase)
            {
                contextFactory.InitializeDatabase();
            }

            ISender sender = container.Resolve<ISender>();
            return sender;
        }
        


        private static List<ComposerSettings<TKey>> GetComposerSettings<TKey>()
            where TKey : struct
        {
            return new List<ComposerSettings<TKey>>()
            {
                new ComposerSettings<TKey>()
                {
                    CategoryId = (int)CategoryTypes.Music,
                    ComposerSettingsId = GenerateKey<TKey>(),
                    Subscription = new SubscriptionParameters()
                    {
                        CategoryId = (int)CategoryTypes.Music
                    },
                    CompositionHandlerId = null,
                    Templates = new List<DispatchTemplate<TKey>>()
                    {
                        new EmailDispatchTemplate<TKey>()
                        {
                            DeliveryType = (int)Model.DeliveryTypes.Console,
                            IsBodyHtml = false,
                            SubjectProvider = (StringTemplate)"subject {key}",
                            SubjectTransformer = new ReplaceTransformer(),
                            BodyProvider = (StringTemplate)"body text {key}",
                            BodyTransformer = new ReplaceTransformer(),
                        },
                        new EmailDispatchTemplate<TKey>()
                        {
                            DeliveryType = (int)Model.DeliveryTypes.Email,
                            IsBodyHtml = false,
                            SubjectProvider = (StringTemplate)"subject {key}",
                            SubjectTransformer = new ReplaceTransformer(),
                            BodyProvider = (StringTemplate)"body text {key}",
                            BodyTransformer = new ReplaceTransformer(),
                        }
                    },
                    Updates = new UpdateParameters()
                },
                new ComposerSettings<TKey>()
                {
                    CategoryId = (int)CategoryTypes.Games,
                    ComposerSettingsId = GenerateKey<TKey>(),
                    Subscription = new SubscriptionParameters()
                    {
                        CategoryId = (int)CategoryTypes.Games
                    },
                    Templates = new List<DispatchTemplate<TKey>>()
                    {
                        new EmailDispatchTemplate<TKey>()
                        {
                            DeliveryType = (int)Model.DeliveryTypes.Console,
                            IsBodyHtml = true,
                            SubjectProvider = new StringTemplate("Demo event in games category"),
                            SubjectTransformer = null,
                            BodyProvider = new StringTemplate("Games-Body.cshtml"),
                            BodyTransformer = new ReplaceTransformer(),
                        }
                    },
                    Updates = new UpdateParameters()
                }
            };
        }
        
        private static TKey GenerateKey<TKey>()
            where TKey : struct
        {
            if (typeof(TKey) == typeof(long))
            {
                _nextComposerSettingsKey++;
                return (TKey)Convert.ChangeType(_nextComposerSettingsKey, typeof(TKey));
            }

            if (typeof(TKey) == typeof(ObjectId))
            {
                return (TKey)Convert.ChangeType(ObjectId.GenerateNewId(), typeof(TKey));
            }

            throw new NotImplementedException($"Type of {typeof(TKey)} is not supported");
        }

        private static void TestBuildAllTemplates<TKey>()
            where TKey : struct
        {
            List<ComposerSettings<TKey>> settings = GetComposerSettings<TKey>();

            foreach (ComposerSettings<TKey> composerSettings in settings)
            {
                foreach (DispatchTemplate<TKey> template in composerSettings.Templates)
                {
                    var subscribers = new List<Subscriber<TKey>>()
                {
                    new Subscriber<TKey>() { }
                };

                    var signalEvent = new SignalEvent<TKey>()
                    {
                        DataKeyValues = new Dictionary<string, string>()
                    {
                        { "key", "Custom value" }
                    }
                    };
                    List<SignalDispatch<TKey>> signals = template.Build(composerSettings, signalEvent, subscribers);
                }
            }
        }
    }

}
