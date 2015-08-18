using Common.Utility;
using SignaloBot.DAL.Context;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.DAL.Queries.Sender;
using SignaloBot.Sender.Dispatcher;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Queue.InsertNotifier;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Senders.Email;
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
        enum DeliveryType { Email };


        //методы
        static void Main(string[] args)
        {
            MessageDispatcher<Signal> dispatcher = InitDispatcher();
            dispatcher.Start();

            Console.ReadLine();
        }

        private static MessageDispatcher<Signal> InitDispatcher()
        {
            ICommonLogger logger = new NLogger();
            string connectionString = SignaloBotTestParameters.ConnectionString;
            string prefix = SignaloBotTestParameters.SqlPrefix;

            var dispatcher = new MessageDispatcher<Signal>();

            dispatcher.Logger = logger;

            dispatcher.MessageProviders.Add(new MessageProvider<Signal>()
            {
                MessageQueue = new MessageQueue<Signal>(new QueueQueries(connectionString, prefix, logger))
                {
                    StorageMessageQueryCount = 200,
                    MaxDeliveryFailedAttempts = 2,
                    FailedAttemptRetryPeriod = TimeSpan.FromMinutes(10)
                },
                StorageInsertNotifier = new SqlInsertNotifier<Signal, SenderDbContext>(
                    logger, connectionString, prefix),
                StorageQueryPeriod = TimeSpan.FromMinutes(5)
            });
            
            dispatcher.SenderProvider.Register((int)DeliveryType.Email,
                new SmtpEmailSender(logger,
                    new SmtpSettings()
                    {
                        Server = DemoParameters.SmtpServer,
                        Port = DemoParameters.Port,
                        EnableSsl = DemoParameters.EnableSsl,
                        Credentials = new NetworkCredential(DemoParameters.Login, DemoParameters.Password)
                    })
                );

            dispatcher.StatisticsCollector = new ConsoleStatisticsCollector();

            return dispatcher;
        }
    }

   
}
