using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DAL.Entities;

namespace Sanatana.Notifications.DispatchHandling.Interrupters
{
    public class NoTimeoutInterrupter<TKey> : IInterrupter<TKey>
        where TKey : struct
    {

        //methods
        public virtual void Success(SignalDispatch<TKey> dispatch)
        {
        }

        public virtual void Fail(SignalDispatch<TKey> dispatch, DispatcherAvailability availability)
        {
        }

        public virtual DateTime? GetTimeoutEndUtc()
        {
            return null;
        }

    }
}
