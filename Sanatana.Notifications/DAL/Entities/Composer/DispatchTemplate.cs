using System.Collections.Generic;
using Sanatana.Notifications.DAL;
using System;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DAL.Entities
{
    public abstract class DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties
        public TKey DispatchTemplateId { get; set; }
        public TKey EventSettingsId { get; set; }
        public string DisplayName { get; set; }
        public int Version { get; set; }
        public int DeliveryType { get; set; }
        public int? ScheduleSet { get; set; }
        public bool IsActive { get; set; } = true;



        //methods
        public abstract List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers);

        protected virtual void SetBaseProperties(SignalDispatch<TKey> dispatch, EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber)
        {
            dispatch.DeliveryType = DeliveryType;
            dispatch.CategoryId = settings.CategoryId;
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