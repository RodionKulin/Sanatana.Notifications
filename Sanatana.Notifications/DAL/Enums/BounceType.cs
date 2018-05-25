using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Enums
{
    /// <summary>
    /// Bounce type that describes reason on message rejection.
    /// </summary>
    public enum BounceType
    {
        /// <summary>
        /// Hard bounce should lead to excluding address from using again.
        /// </summary>
        HardBounce,

        /// <summary>
        /// Soft bounce means failure to deliver with possibility to try again.
        /// </summary>
        SoftBounce,

        /// <summary>
        /// Unknown bounce type.
        /// </summary>
        Unknown
    }
}
