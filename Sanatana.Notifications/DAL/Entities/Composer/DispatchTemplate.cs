using System.Collections.Generic;
using System;
using Sanatana.Notifications.DAL.Results;
using Sanatana.Notifications.EventsHandling.Templates;
using System.Linq;

namespace Sanatana.Notifications.DAL.Entities
{
    public abstract class DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties
        /// <summary>
        /// Identifier. Only required if stored in database. Is set by database itself.
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
        /// Optional identifier. If not null enables scheduling of dispatches for later sending. ScheduleSet is used to match SubscriberScheduleSettings set of daytime intervals that user chosen to receive notifications.
        /// </summary>
        public int? ScheduleSet { get; set; }
        /// <summary>
        /// Toggle to enable or disable building SignalDispatches from this Template. Often used in conjunction with storing EventSettings and DispatchTemplates in database.
        /// </summary>
        public bool IsActive { get; set; } = true;



        //methods
        public abstract List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent,
             List<Subscriber<TKey>> subscribers, List<TemplateData> dataWithCulture);

        protected virtual List<string> FillTemplate(ITemplateProvider provider, ITemplateTransformer transformer,
            List<Subscriber<TKey>> subscribers, List<TemplateData> languageTemplateData)
        {
            if(provider == null || transformer == null)
            {
                return subscribers
                    .Select(x => string.Empty)
                    .ToList();
            }

            Dictionary<string, string> filledTemplates = transformer.Transform(provider, languageTemplateData);
            return subscribers
                .Select(subscriber => filledTemplates[subscriber.Language ?? ""])
                .ToList();
        }

        protected virtual void SetBaseProperties(SignalDispatch<TKey> dispatch, EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber)
        {
            dispatch.EventKey = settings.EventKey;
            dispatch.DeliveryType = DeliveryType;
            dispatch.CategoryId = settings.Subscription.CategoryId;
            dispatch.TopicId = signalEvent.TopicId;

            dispatch.ReceiverSubscriberId = subscriber.SubscriberId;
            dispatch.ReceiverAddress = subscriber.Address;

            dispatch.ScheduleSet = ScheduleSet;
            dispatch.IsScheduled = false;
            dispatch.CreateDateUtc = signalEvent.CreateDateUtc;
            dispatch.SendDateUtc = DateTime.UtcNow;
            dispatch.FailedAttempts = 0;
        }
    }
}