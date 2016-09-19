using MongoDB.Bson;
using SignaloBot.Sender.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL;
using SignaloBot.Sender.Processors;

namespace SignaloBot.Demo.Sender
{
    public class ConsoleMessageSender<TKey> : IDispatcher<TKey>
        where TKey : struct
    {

        //методы
        public ProcessingResult Send(SignalDispatchBase<TKey> item)
        {
            SubjectDispatch<TKey> signal = item as SubjectDispatch<TKey>;

            Console.WriteLine("{0}: Send console message with subject: {1} and text: {2}"
                , DateTime.Now.ToLongTimeString(), signal.MessageSubject, signal.MessageBody);
            return ProcessingResult.Success;
        }

        public DispatcherAvailability CheckAvailability()
        {
            return DispatcherAvailability.Available;
        }

        public void Dispose()
        {
        }

    }
}
