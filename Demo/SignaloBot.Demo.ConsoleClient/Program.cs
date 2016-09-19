using SignaloBot.Demo.ConsoleClient.SignaloBot.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Demo.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to send or enter to exit.");
            
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            while(keyInfo.Key != ConsoleKey.Enter)
            {
                Send();
                Console.WriteLine("{0:HH:mm:ss} Event sended", DateTime.Now);
                keyInfo = Console.ReadKey(true);
            }

        }

        private static void Send()
        {
            var factory = new ChannelFactory<ISignalServiceOf_ObjectId>(
                "NetNamedPipeBinding_ISignalServiceOf_ObjectId");
            ISignalServiceOf_ObjectId proxy = factory.CreateChannel();
                        
            proxy.RaiseKeyValueEvent(null, null, 1, null, new Dictionary<string, string>()
            {
                {  "key", "value" }
            });
                
            ((IClientChannel)proxy).Close();
            factory.Close();
        }
    }
}
