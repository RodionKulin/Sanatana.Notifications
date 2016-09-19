using MongoDB.Bson;
using SignaloBot.Sender.Queue;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL;
using SignaloBot.Sender;
using SignaloBot.Sender.Processors;
using Common.Utility;

namespace SignaloBot.Demo.Sender
{
    public class ConsoleStatisticsCollector<TKey> : IStatisticsCollector<TKey>
            where TKey : struct
    {
        public void HubSwitched(bool switchedOn)
        {
            Console.WriteLine("{0}: Dispatcher switched {1}."
                , DateTime.Now.ToLongTimeString(), switchedOn ? "on" : "off");
        }
        

        public void EventTransferred(SignalEventBase<TKey> item)
        {
            
            Console.WriteLine("{0}: Event received from client in category: {1}."
               , DateTime.Now.ToLongTimeString(), item.CategoryID);
        }

        public void EventStorageQueried(
            TimeSpan time, QueryResult<List<SignalEventBase<TKey>>> items)
        {
            if (items.Result != null && items.Result.Count > 0)
            {
                Console.WriteLine("{0}: Event storage query time {1} for {2} items."
               , DateTime.Now.ToLongTimeString(), time, items.Result.Count);
            }
        }

        public void DispatchStorageQueried(
            TimeSpan time, QueryResult<List<SignalDispatchBase<TKey>>> items)
        {
            if(items.Result != null && items.Result.Count > 0)
            {
                Console.WriteLine("{0}: Dispatch storage query time {1} for {2} items."
                   , DateTime.Now.ToLongTimeString(), time, items.Result.Count);
            }
        }

        public void DispatchesComposed(SignalEventBase<TKey> item, TimeSpan time
            , ProcessingResult composeResult)
        {
            if(composeResult != ProcessingResult.Success)
            {
                Console.WriteLine("{0}: Dispatches composed with result {1} in {2}."
                        , DateTime.Now.ToLongTimeString(), composeResult, time);
            }
        }

        public void DispatchSended(SignalDispatchBase<TKey> item, TimeSpan time
            , ProcessingResult sendResult, DispatcherAvailability senderAvailability)
        {
            if (sendResult != ProcessingResult.Success)
            {
                Console.WriteLine("{0}: Dispatch processed with result {1} in {2}."
                   , DateTime.Now.ToLongTimeString(), sendResult, time);
            }
            
        }


        public void Dispose()
        {
        }

    }
}
