using Sanatana.Timers.Switchables;
using System;

namespace Sanatana.Notifications.Sender
{
    public interface ISender : IDisposable
    {
        SwitchState State { get; }
        void Start();
        void Stop(bool blockThread, TimeSpan? timeout = null);
    }
}