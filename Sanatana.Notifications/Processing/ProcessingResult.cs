using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Processing
{
    public enum ProcessingResult
    {
        Success,
        Fail,
        Repeat,
        NoHandlerFound,
        ReturnToStorage
    }
}
