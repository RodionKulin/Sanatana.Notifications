using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders
{
    public interface ISignalProviderControl
    {
        void Start();
        void Stop(TimeSpan? timeout);
    }
}
