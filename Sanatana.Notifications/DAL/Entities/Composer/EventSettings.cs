using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    public class EventSettings<TKey>
        where TKey : struct
    {
        /// <summary>
        /// Required unique identifier. Assigned by database if database storage is chosen. By default should be assigned manually cause settings are stored in memory.
        /// </summary>
        public TKey EventSettingsId { get; set; }
        /// <summary>
        /// Optional field for display in UI.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Identifier used to match incoming SignalEvent to EventSettings. SignalEvents can come from IDirectSignalProvider, WcfSignalProvider or other SignalProviders including custom.
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Optional identifier to match EventSettings with custom ICompositionHandler. Default ICompositionHandler is used if value if left null. If you need to override selecting subscribers, building dispatches, scheduling and updates you can do it via appropriate database queries interfaces or by overriding whole ICompositionHandler that is doing the pipelining.
        /// </summary>
        public int? CompositionHandlerId { get; set; }
        /// <summary>
        /// Builders that hold required configuration to construct SignalDispatch that will be send.
        /// </summary>
        public List<DispatchTemplate<TKey>> Templates { get; set; }
        /// <summary>
        /// Query settings describing how to select subscribers that will be notified.
        /// </summary>
        public SubscriptionParameters Subscription { get; set; }
        /// <summary>
        /// Query settings to update data for subscribers after receiving a SignalEvent. Update data like incrementing a number of messages sent and latest send time.
        /// </summary>
        public UpdateParameters Updates { get; set; }
    }
}
