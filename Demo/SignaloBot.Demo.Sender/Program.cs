using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using SignaloBot.DAL;
using SignaloBot.DAL.MongoDb;
using SignaloBot.Sender;
using SignaloBot.Sender.Composers;
using SignaloBot.Sender.Composers.Templates;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Senders.Email;
using SignaloBot.Sender.Service;
using SignaloBot.Sender.Statistics;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.TestParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Demo.Sender
{
    class Program
    {
        private enum DeliveryType { Console, Email };
        private enum Category { Videos, Music, Games }


        //методы
        static void Main(string[] args)
        {
            SignaloBotHub<ObjectId> hub = InitSenderHub();

            hub.Start();
            Console.WriteLine("Started. Press enter to stop.");
            Console.ReadLine();
            
            hub.Stop(true);
        }
        
        private static SignaloBotHub<ObjectId> InitSenderHub()
        {
            ICommonLogger logger = new NLogger();
            MongoDbConnectionSettings connection = SignaloBotTestParameters.MongoDbConnection;

            var queueQueries = new MongoDbSignalDispatchQueries(logger, connection);
            var eventQueries = new MongoDbSignalEventQueries(logger, connection);
            var deliveryTypeSettingsQueries = new MongoDbUserDeliveryTypeSettingsQueries(logger, connection);
            var categoryQueries = new MongoDbUserCategorySettingsQueries(logger, connection);
            var topicQueries = new MongoDbUserTopicSettingsQueries(logger, connection);
            var receivePeriodQueries = new MongoDbUserReceivePeriodQueries(logger, connection);

            var hub = new SignaloBotHub<ObjectId>();
            hub.Logger = logger;
            hub.StatisticsCollector = new ConsoleStatisticsCollector<ObjectId>();

            hub.EventQueues.Add(new EventQueue<ObjectId>(eventQueries)
            {
                ItemsQueryCount = 200,
                QueryPeriod = TimeSpan.FromSeconds(30),
                MaxFailedAttempts = 2
            });

            hub.Composer = new KeyValueComposer<ObjectId>()
            {
                ComposerQueries = new LocalComposerQueries<ObjectId>(GetComposerSettings()),
                SubscriberQueries = new MongoDbSubscriberQueries(logger, connection),
                Logger = logger
            };

            hub.DispatchQueues.Add(new DispatchQueue<ObjectId>(queueQueries)
            {
                ItemsQueryCount = 200,
                QueryPeriod = TimeSpan.FromSeconds(30),
                MaxFailedAttempts = 2,
                FailedAttemptRetryPeriod = TimeSpan.FromMinutes(5)
            });

            hub.Senders.Add(new DispatchChannel<ObjectId>()
            {
                Sender = new ConsoleMessageSender<ObjectId>(),
                DeliveryType = (int)DeliveryType.Console
            });

            var instanceProvider = new SignalServiceInstanceProvider<ObjectId>(
                hub.EventQueues.First(), hub.StatisticsCollector
                , deliveryTypeSettingsQueries, categoryQueries, topicQueries, receivePeriodQueries);
            hub.ServiceHost = new SignalServiceSelfHost<ObjectId>(logger, instanceProvider);

            return hub;
        }

        private static List<ComposerSettings<ObjectId>> GetComposerSettings()
        {
            return new List<ComposerSettings<ObjectId>>()
            {
                new ComposerSettings<ObjectId>()
                {
                    CategoryID = (int)Category.Music,
                    ComposerSettingsID = ObjectId.GenerateNewId(),
                    Subscribtion = new SubscribtionParameters()
                    {
                        CategoryID = (int)Category.Music
                    },
                    Templates = new List<SignalTemplateBase<ObjectId>>()
                    {
                        new SubjectDispatchTemplate<ObjectId>()
                        {
                            DeliveryType = (int)DeliveryType.Console,
                            CategoryID = (int)Category.Music,
                            SenderAddress = DemoParameters.EmailFromAddress,
                            SenderDisplayName = "SignaloBot sender", //optional
                            IsBodyHtml = false,
                            SubjectProvider = (StringTemplate)"subject {key}",
                            SubjectTransformer = new ReplaceTransformer(),
                            BodyProvider = (StringTemplate)"body text {key}",
                            BodyTransformer = new ReplaceTransformer(),
                            
                        }
                    }
                },
                new ComposerSettings<ObjectId>()
                {
                    CategoryID = (int)Category.Games,
                    ComposerSettingsID = ObjectId.GenerateNewId(),
                    Subscribtion = new SubscribtionParameters()
                    {
                        CategoryID = (int)Category.Games
                    },
                    Templates = new List<SignalTemplateBase<ObjectId>>()
                    {
                        new SubjectDispatchTemplate<ObjectId>()
                        {
                            CategoryID = (int)Category.Games,
                            DeliveryType = (int)DeliveryType.Console,
                            SenderAddress = DemoParameters.EmailFromAddress,
                            SenderDisplayName = "SignaloBot sender",
                            IsBodyHtml = true,
                            SubjectProvider = new StringTemplate("Demo event in games category"),
                            SubjectTransformer = null,
                            BodyProvider = new StringTemplate("Games-Body.cshtml"),
                            BodyTransformer = new RazorTransformer("Templates"),
                        }
                    }
                }
            };
        }
                
        private static void TestBuildAllTemplates()
        {
            List<ComposerSettings<ObjectId>> settings = GetComposerSettings();

            foreach (ComposerSettings<ObjectId> composer in settings)
            {
                foreach (SignalTemplateBase<ObjectId> template in composer.Templates)
                {
                    var subscribers = new List<Subscriber<ObjectId>>()
                    {
                        new Subscriber<ObjectId>() { }
                    };
                    var data = new Dictionary<string, string>();
                    List<SignalDispatchBase<ObjectId>> signals = template.Build(subscribers, data);
                }
            }
        }
    }

   
}
