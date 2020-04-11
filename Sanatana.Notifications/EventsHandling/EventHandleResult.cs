using Sanatana.Notifications.Models;
using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling
{
    public class EventHandleResult<T>
    {
        //properties
        public bool IsFinished { get; set; }
        public ProcessingResult Result { get; set; }
        public List<T> Items { get; set; }


        //init
        public static EventHandleResult<T> FromResult(ProcessingResult result)
        {
            return new EventHandleResult<T>()
            {
                Result = result
            };
        }
    }
}
