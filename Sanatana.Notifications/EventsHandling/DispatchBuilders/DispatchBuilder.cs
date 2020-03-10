using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling
{
    public class DispatchBuilder<TKey> : IDispatchBuilder<TKey>
        where TKey : struct
    {

        //init
        public DispatchBuilder()
        {
        }


        //methods
        public virtual EventHandleResult<SignalDispatch<TKey>> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            var dispatches = new List<SignalDispatch<TKey>>();

            Dictionary<int, List<Subscriber<TKey>>> delivTypeSubscribers = subscribers
                .GroupBy(x => x.DeliveryType)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (DispatchTemplate<TKey> template in settings.Templates)
            {
                if (template.IsActive == false
                    || delivTypeSubscribers.ContainsKey(template.DeliveryType) == false)
                {
                    continue;
                }

                List<Subscriber<TKey>> templateSubscribers = delivTypeSubscribers[template.DeliveryType];
                List<SignalDispatch<TKey>> templateDispatches = BuildTemplate(settings, signalEvent, template, templateSubscribers);
                dispatches.AddRange(templateDispatches);
            }

            return new EventHandleResult<SignalDispatch<TKey>>()
            {
                Items = dispatches,
                IsFinished = true,
                Result = ProcessingResult.Success
            };
        }

        protected virtual List<SignalDispatch<TKey>> BuildTemplate(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , DispatchTemplate<TKey> template , List<Subscriber<TKey>> templateSubscribers)
        {
            return template.Build(settings, signalEvent, templateSubscribers);
        }

    }
}
