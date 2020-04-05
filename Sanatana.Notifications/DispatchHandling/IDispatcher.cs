using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling
{
    public interface IDispatcher<TKey> : IDisposable
        where TKey : struct
    {
        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<ProcessingResult> Send(SignalDispatch<TKey> item);

        /// <summary>
        /// Check availability of dispatcher. Method is called in case of failed send attempt.
        /// </summary>
        /// <returns></returns>
        Task<DispatcherAvailability> CheckAvailability();
    }
}
