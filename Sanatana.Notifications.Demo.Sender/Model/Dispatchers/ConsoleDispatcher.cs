using Sanatana.Notifications.Resources;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DispatchHandling.DeliveryTypes;

namespace Sanatana.Notifications.Demo.Sender.Model
{
    public class ConsoleDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //methods
        public virtual Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            string json = Serialize(item);
            string message = string.Format(MonitorMessages.TraceDispatcher_DispatchReceived
                , DateTime.Now.ToLongTimeString(), json);
            Console.WriteLine(message);
            return Task.FromResult(ProcessingResult.Success);
        }

        protected virtual string Serialize(SignalDispatch<TKey> item)
        {
            string json = "{}";

            using (MemoryStream ms = new MemoryStream())
            {
                Type itemType = item.GetType();
                var ser = new DataContractJsonSerializer(itemType, new DataContractJsonSerializerSettings()
                {
                    MaxItemsInObjectGraph = int.MaxValue
                });
                ser.WriteObject(ms, item);

                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                byte[] msBytes = ms.ToArray();
                json = Encoding.Default.GetString(msBytes);
            }

            return json;
        }

        public virtual Task<DispatcherAvailability> CheckAvailability()
        {
            return Task.FromResult(DispatcherAvailability.Available);
        }

        public virtual void Dispose()
        {
        }

    }
}
