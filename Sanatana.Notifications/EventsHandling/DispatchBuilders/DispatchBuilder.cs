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
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Sanatana.Notifications.EventsHandling
{
    public class DispatchBuilder<TKey> : IDispatchBuilder<TKey>
        where TKey : struct
    {
        //fields
        protected ILogger _logger;


        //ctor
        public DispatchBuilder(ILogger logger)
        {
            _logger = logger;
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
                List<TemplateData> cultureAndData = PrepareCultureData(signalEvent, templateSubscribers);
                List<SignalDispatch<TKey>> templateDispatches = BuildTemplate(settings, signalEvent, template, templateSubscribers, cultureAndData);
                dispatches.AddRange(templateDispatches);
            }

            return new EventHandleResult<SignalDispatch<TKey>>()
            {
                Items = dispatches,
                IsFinished = true,
                Result = ProcessingResult.Success
            };
        }

        protected virtual List<TemplateData> PrepareCultureData(SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            return subscribers
                .Select(x => x.Language)
                .Distinct()
                .ToDictionary(lang => lang, cultureName =>
                {
                    try
                    {
                        return string.IsNullOrEmpty(cultureName) ? null : new CultureInfo(cultureName);
                    }
                    catch (CultureNotFoundException ex)
                    {
                        TKey subscriberId = subscribers.First(x => x.Language == cultureName).SubscriberId;
                        _logger.LogError(ex, SenderInternalMessages.DispatchBuilder_CultureNotFound, cultureName, subscriberId);
                        return null;
                    }
                })
                .Select(culture => new TemplateData(signalEvent.TemplateData, culture.Value, culture.Key))
                .ToList();
        }
               
        protected virtual List<SignalDispatch<TKey>> BuildTemplate(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , DispatchTemplate<TKey> template, List<Subscriber<TKey>> subscribers, List<TemplateData> cultureAndData)
        {
            return template.Build(settings, signalEvent, subscribers, cultureAndData);
        }

    }
}
