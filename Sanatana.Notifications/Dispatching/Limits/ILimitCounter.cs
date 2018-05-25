using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Dispatching.Limits
{
    public interface ILimitCounter : IDisposable
    {
        /// <summary>
        /// Increment counter of dispatches sent.
        /// </summary>
        void InsertTime();

        /// <summary>
        /// Get time of restriction pause end.
        /// </summary>
        /// <returns></returns>
        DateTime? GetLimitsEndTimeUtc();

        /// <summary>
        /// Get dispatch amount capacity.
        /// </summary>
        /// <returns></returns>
        int GetLimitCapacity();
    }
}
