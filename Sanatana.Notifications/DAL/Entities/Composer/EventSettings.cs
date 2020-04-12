using Sanatana.Notifications.EventsHandling.Templates;
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
        /// Required unique identifier. Assigned by database if database storage is chosen. 
        /// Should be assigned a static value manually if settings are stored in memory (default) to match SignalEvent with EventSettings.
        /// </summary>
        public TKey EventSettingsId { get; set; }
        /// <summary>
        /// Optional field for display in UI.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Key used to match incoming SignalEvent to EventSettings. SignalEvents can come from IDirectSignalProvider, WcfSignalProvider or other ISignalProvider.
        /// </summary>
        public int EventKey { get; set; }
        /// <summary>
        /// Optional identifier to match EventSettings with custom ICompositionHandler. 
        /// Default IEventHandler is used if value if null. 
        /// If you need to override selecting subscribers, building dispatches, scheduling and updates you can do it via appropriate database queries interfaces 
        /// or by overriding whole IEventHandler that is doing the pipelining and put here it's new EventHandlerId.
        /// </summary>
        public int? EventHandlerId { get; set; }
        /// <summary>
        /// Templates that hold required configuration to construct SignalDispatch that will be send.
        /// </summary>
        public List<DispatchTemplate<TKey>> Templates { get; set; }
        /// <summary>
        /// Query settings for selecting subscribers to notify.
        /// </summary>
        public SubscriptionParameters Subscription { get; set; }
        /// <summary>
        /// Query settings to update subscriber's settings and counters after receiving a SignalEvent. 
        /// </summary>
        public UpdateParameters Updates { get; set; }
        /// <summary>
        /// Enable storing dispatch in history database table after sending it.
        /// </summary>
        public bool StoreInHistory { get; set; }
        /// <summary>
        /// Optional identifier of ITemplateDataConsolidator that will combine multiple TemplateData objects into one.
        /// Before sending Dispatch will get all queued dispatches for subscriber of same category and create a single Dispatch from them.
        /// Null disabled consolidation.
        /// </summary>
        public int? ConsolidatorId { get; set; }
    }
}
