using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public enum AddresseeType
    {
        /// <summary>
        /// Match subscribers using SubscriptionParameters
        /// </summary>
        SubscriptionParameters,
        /// <summary>
        /// Find subscribers by provided SubscriberIds
        /// </summary>
        SubscriberIds,
        /// <summary>
        /// Send dispatches to provided addresses
        /// </summary>
        DirectAddresses
    }
}
