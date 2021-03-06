﻿using Sanatana.Notifications.EventsHandling;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    public class StoredNotificationTemplate<TKey> : DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties  
        public ITemplateProvider SubjectProvider { get; set; }
        public ITemplateTransformer SubjectTransformer { get; set; }
        public ITemplateProvider BodyProvider { get; set; }
        public ITemplateTransformer BodyTransformer { get; set; }



        //methods
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent,
             List<Subscriber<TKey>> subscribers, List<TemplateData> languageTemplateData)
        {
            List<string> subjects = FillTemplateProperty(SubjectProvider, SubjectTransformer, subscribers, languageTemplateData);
            List<string> bodies = FillTemplateProperty(BodyProvider, BodyTransformer, subscribers, languageTemplateData);

            return subscribers
                .Select((subscriber, i) => AssembleStoredNotification(settings, signalEvent, subscriber, subjects[i], bodies[i]))
                .Cast<SignalDispatch<TKey>>()
                .ToList();
        }

        protected virtual StoredNotificationDispatch<TKey> AssembleStoredNotification(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber, string subject, string body)
        {
            var dispatch = new StoredNotificationDispatch<TKey>()
            {
                MessageSubject = subject,
                MessageBody = body
            };

            SetBaseProperties(dispatch, settings, signalEvent, subscriber);
            return dispatch;
        }

        public override void Update(SignalDispatch<TKey> item, TemplateData templateData)
        {
            var dispatch = (StoredNotificationDispatch<TKey>)item;
            dispatch.MessageSubject = FillTemplateProperty(SubjectProvider, SubjectTransformer, templateData);
            dispatch.MessageBody = FillTemplateProperty(BodyProvider, BodyTransformer, templateData);
        }
    }
}
