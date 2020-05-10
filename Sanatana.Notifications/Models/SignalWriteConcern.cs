using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.Models
{
    public enum SignalWriteConcern
    {
        /// <summary>
        /// Use default Sender settings
        /// </summary>
        Default,
        /// <summary>
        /// Store Signal in memory queue before it will be send.
        /// If Sender instance will be shut down ungracefully, then Signals in memory will be lost.
        /// </summary>
        MemoryOnly,
        /// <summary>
        /// Immediately save Signal to database on receiving. This ensures that Signal will be processed after Sender ungraceful shutdown.
        /// </summary>
        PersistentStorage
    }
}
