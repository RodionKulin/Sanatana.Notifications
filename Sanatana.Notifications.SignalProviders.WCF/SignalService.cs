using Sanatana.Notifications.Queues;
using Sanatana.Notifications.Monitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Sanatana.Notifications.Sender;
using Sanatana.Notifications.DAL.Interfaces;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [ServiceKnownType(nameof(SenderSettings.GetKnownServiceTypes), typeof(SenderSettings))]
    public class SignalService<TKey> : BaseSignalProvider<TKey>, ISignalService<TKey>
        where TKey : struct
    {
        public SignalService(IEventQueue<TKey> eventQueue, IDispatchQueue<TKey> dispatchQueue, IMonitor<TKey> eventSink,
            ISignalEventQueries<TKey> eventQueries, ISignalDispatchQueries<TKey> dispatchQueries, SenderSettings senderSettings)
            : base(eventQueue, dispatchQueue, eventSink, eventQueries, dispatchQueries, senderSettings)
        {
        }
    }
}
