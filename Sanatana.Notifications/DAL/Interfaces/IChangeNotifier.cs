using System;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface IChangeNotifier<TEntity> : IDisposable
    {
        bool HasUpdates { get; set; }
        void StartMonitor();
    }
}
