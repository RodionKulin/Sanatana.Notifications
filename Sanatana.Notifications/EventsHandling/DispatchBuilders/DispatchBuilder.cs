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
using Newtonsoft.Json;
using Sanatana.Notifications.Models;

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
                List<TemplateData> templateData = PrepareTemplateData(signalEvent, templateSubscribers);
                List<SignalDispatch<TKey>> templateDispatches = BuildTemplate(settings, signalEvent, template, templateSubscribers, templateData);
                dispatches.AddRange(templateDispatches);
            }

            return new EventHandleResult<SignalDispatch<TKey>>()
            {
                Items = dispatches,
                IsFinished = true,
                Result = ProcessingResult.Success
            };
        }

        protected virtual List<TemplateData> PrepareTemplateData(SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            //deserialize object model
            object objectModel = null;
            try
            {
                objectModel = string.IsNullOrEmpty(signalEvent.TemplateDataObj)
                    ? null
                    : JsonConvert.DeserializeObject(signalEvent.TemplateDataObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, SenderInternalMessages.Common_DeserializeError,
                    nameof(signalEvent.TemplateDataObj), signalEvent.TemplateDataObj,
                    nameof(SignalEvent<TKey>), signalEvent.SignalEventId);
            }

            //find distinct languages
            List<TemplateData> templatesData = subscribers
                .Select(x => x.Language ?? string.Empty)
                .Distinct()
                .Select(language => new TemplateData(signalEvent.TemplateDataDict, objectModel, language: language))
                .ToList();

            //validate and log invalid language values.
            foreach (TemplateData templateData in templatesData)
            {
                //empty language is not logged. 
                //template provider will return default language template in this case.
                if (string.IsNullOrEmpty(templateData.Language))
                {
                    continue;
                }

                try
                {
                    //language should be a valid culture name
                    new CultureInfo(templateData.Language);
                }
                catch (CultureNotFoundException ex)
                {
                    TKey subscriberId = subscribers.First(x => x.Language == templateData.Language).SubscriberId;
                    _logger.LogError(ex, SenderInternalMessages.DispatchBuilder_CultureNotFound, templateData.Language, subscriberId);
                }
            }

            return templatesData;
        }
               
        protected virtual List<SignalDispatch<TKey>> BuildTemplate(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , DispatchTemplate<TKey> template, List<Subscriber<TKey>> subscribers, List<TemplateData> templateData)
        {
            return template.Build(settings, signalEvent, subscribers, templateData);
        }

    }
}
