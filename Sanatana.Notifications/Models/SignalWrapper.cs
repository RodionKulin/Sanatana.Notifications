using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Models
{
    public class SignalWrapper<T>
    {
        //properties
        public T Signal { get; set; }
        public bool IsPermanentlyStored { get; set; }
        public bool IsUpdated { get; set; }
        public Guid? TempStorageId { get; set; }


        //init
        public SignalWrapper(T signal, bool isPermanentlyStored)
        {
            Signal = signal;
            IsPermanentlyStored = isPermanentlyStored;
        }
        
    }
}
