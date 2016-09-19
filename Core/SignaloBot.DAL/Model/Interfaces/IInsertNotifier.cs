using System;

namespace SignaloBot.DAL
{
    public interface IInsertNotifier : IDisposable
    {
        bool HasUpdates { get; set; }
        void StartMonitor();
        void StopMonitor();
    }
}
