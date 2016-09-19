using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Processors
{
    public enum ProcessingResult
    {
        Success,
        Fail,
        Repeat,
        NoHandlerFound,
        Return
    }
}
