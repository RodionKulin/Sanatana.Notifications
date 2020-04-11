using Sanatana.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing
{
    public interface ISignalFlushJob<TSignal>
    {
        void Delete(SignalWrapper<TSignal> item);
        void Return(SignalWrapper<TSignal> item);
    }
}
