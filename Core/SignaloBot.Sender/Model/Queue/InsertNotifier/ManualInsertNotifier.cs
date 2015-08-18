using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue.InsertNotifier
{
    public class ManualInsertNotifier : IStorageInsertNotifier
    {
        //свойства
        public bool HasUpdates { get; set; }


        //методы
        public void StartMonitor()
        {
        }

        public void StopMonitor()
        {
        }


        //IDisposable
        public void Dispose()
        {
        }
    }
}
