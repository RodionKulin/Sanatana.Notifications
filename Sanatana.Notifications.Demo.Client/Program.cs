using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Demo.Client
{
    class Program
    {
        private enum Category { Videos, Music, Games }

        static void Main(string[] args)
        {
            //Console.WriteLine("Press any key to send or enter to exit.");
            
            //ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            //while(keyInfo.Key != ConsoleKey.Enter)
            //{
            //    Console.WriteLine("{0:HH:mm:ss} Event sending started", DateTime.Now);
            //    Send();
            //    keyInfo = Console.ReadKey(true);
            //}
        }

        //private static void Send()
        //{
        //    string endpointConfigurationName = "NetNamedPipeBinding_ISignalServiceOf_Int64";
        //    var endpointService = new SignalEndpointService(endpointConfigurationName);

        //    int category = (int)Category.Music;
        //    Exception exception;
        //    var data = new Dictionary<string, string>(){
        //        {  "key", "value" }
        //    };
        //    bool transferred = endpointService.RaiseEventFromSubscriptions(data, category);
        //}
    }
}
