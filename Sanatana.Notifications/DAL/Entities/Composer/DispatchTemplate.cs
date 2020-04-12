using System.Collections.Generic;
using System;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.EventsHandling.Templates;
using System.Linq;
using Newtonsoft.Json;

namespace Sanatana.Notifications.DAL.Entities
{
    /// <summary>
    /// Entity containing properties that are copied to Dispatches created from it.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties
        /// <summary>
        /// Required unique identifier. Assigned by database if database storage is chosen. 
        /// Should be assigned a static value manually if settings are stored in memory (default) to match SignalDispatch with DispatchTemplate.
        /// </summary>
        public TKey DispatchTemplateId { get; set; }
        /// <summary>
        /// Foreign key to join with EventSettings. Only required if stored in database. Is set in database queries layer.
        /// </summary>
        public TKey EventSettingsId { get; set; }
        /// <summary>
        /// Optional field for display in UI.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Delivery type identifier to assign to SignalDispatch. Will be used to match constructed SignalDispatch with DispatchChannel.
        /// </summary>
        public int DeliveryType { get; set; }
        /// <summary>
        /// Optional identifier. If not null enables scheduling of dispatches for later sending. 
        /// ScheduleSet is used to match SubscriberScheduleSettings set of daytime intervals that user choose to receive notifications.
        /// </summary>
        public int? ScheduleSet { get; set; }
        /// <summary>
        /// Toggle to enable or disable building SignalDispatches from this Template. 
        /// </summary>
        public bool IsActive { get; set; } = true;
     


        //methods
        public abstract List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent,
             List<Subscriber<TKey>> subscribers, List<TemplateData> dataWithLanguage);

        public abstract void Update(SignalDispatch<TKey> item, TemplateData templateData);

        protected virtual List<string> FillTemplateProperty(ITemplateProvider provider, ITemplateTransformer transformer,
            List<Subscriber<TKey>> subscribers, List<TemplateData> templateData)
        {
            if(provider == null || transformer == null)
            {
                return subscribers
                    .Select(x => (string)null)
                    .ToList();
            }

            Dictionary<string, string> filledTemplates = transformer.Transform(provider, templateData);
            return subscribers
                .Select(subscriber => filledTemplates[subscriber.Language ?? string.Empty])
                .ToList();
        }

        protected virtual string FillTemplateProperty(ITemplateProvider provider, ITemplateTransformer transformer,
            TemplateData templateData)
        {
            if (provider == null || transformer == null)
            {
                return null;
            }

            Dictionary<string, string> filledTemplates = transformer.Transform(
                provider, new List<TemplateData> { templateData });
            return filledTemplates[templateData.Language ?? string.Empty];
        }

        protected virtual void SetBaseProperties(SignalDispatch<TKey> dispatch, EventSettings<TKey> settings,
            SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber)
        {
            dispatch.EventSettingsId = settings.EventSettingsId;
            dispatch.DispatchTemplateId = DispatchTemplateId;

            dispatch.DeliveryType = DeliveryType;
            dispatch.CategoryId = settings.Subscription.CategoryId;
            dispatch.TopicId = signalEvent.TopicId ?? settings.Subscription.TopicId;

            dispatch.ReceiverSubscriberId = subscriber.SubscriberId;
            dispatch.ReceiverAddress = subscriber.Address;

            dispatch.ScheduleSet = ScheduleSet;
            dispatch.IsScheduled = false;

            dispatch.CreateDateUtc = signalEvent.CreateDateUtc;
            dispatch.SendDateUtc = DateTime.UtcNow;
            dispatch.FailedAttempts = 0;

            dispatch.Language = subscriber.Language ?? string.Empty;
            if (settings.ConsolidatorId != null)
            {
                dispatch.TemplateData = signalEvent.TemplateDataObj == null
                    ? JsonConvert.SerializeObject(signalEvent.TemplateDataDict)
                    : signalEvent.TemplateDataObj;
            }
        }
    }
}