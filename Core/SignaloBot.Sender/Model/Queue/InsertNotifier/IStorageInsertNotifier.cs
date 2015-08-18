using System;

namespace SignaloBot.Sender.Queue.InsertNotifier
{
    public interface IStorageInsertNotifier : IDisposable
    {
        bool HasUpdates { get; set; }
        void StartMonitor();
        void StopMonitor();
    }
}
