using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Composing
{
    public class ComposeResult<T>
    {
        //properties
        public bool IsFinished { get; set; }
        public ProcessingResult Result { get; set; }
        public List<T> Items { get; set; }


        //init
        public static ComposeResult<T> FromResult(ProcessingResult result)
        {
            return new ComposeResult<T>()
            {
                Result = result
            };
        }
    }
}
