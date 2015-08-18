using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Demo.Sender
{
    public class ConsoleStatisticsCollector : IStatisticsCollector<Signal>
    {
        public void DispatcherSwitched(bool switchedOn)
        {
            Console.Write("Dispatcher switched on.");
            Console.WriteLine();
        }

        public void MessageProcessed(SendChannel<Signal> sendChannel, Signal message, SendResult sendResult, TimeSpan time)
        {
            Console.Write("Message processed with result: {0} in {1}.", sendResult, time);
            Console.WriteLine();
        }

        public void StorageQueried(MessageProvider<Signal> messageProvider, TimeSpan time)
        {
            Console.Write("Storage query time: {0}.", time);
            Console.WriteLine();
        }

        public void Dispose()
        {
        }
    }
}
