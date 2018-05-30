using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Service;

namespace Sanatana.Notifications.Demo.Client
{
    class Program
    {
        public enum CategoryTypes
        {
            CustomerGreetings,
            BasketReminder,
            PurchaseApprovement
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to send or enter to exit.");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                Console.WriteLine("{0:HH:mm:ss} Transfer event data", DateTime.Now);
                Send();
                keyInfo = Console.ReadKey(true);
            }
        }

        private static void Send()
        {
            var endpointService = new SignalServiceOf_Int64Client(
                SignalServiceOf_Int64Client.EndpointConfiguration.MainTcpEndpoint);

            int category = (int)CategoryTypes.CustomerGreetings;
            var data = new Dictionary<string, string>(){
                {  "customer", "Crabs" }
            };
            endpointService.RaiseEventAndMatchSubscribersAsync(data, category, null, null).Wait();
        }
    }
}
