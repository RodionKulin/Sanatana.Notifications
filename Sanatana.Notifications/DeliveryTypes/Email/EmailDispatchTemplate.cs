using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.EventsHandling.Templates;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DeliveryTypes.Email
{
    public class EmailDispatchTemplate<TKey> : DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties  
        public ITemplateProvider SubjectProvider { get; set; }
        public ITemplateTransformer SubjectTransformer { get; set; }
        public ITemplateProvider BodyProvider { get; set; }
        public ITemplateTransformer BodyTransformer { get; set; }
        public bool IsBodyHtml { get; set; }
        public List<string> CCAddresses { get; set; }
        public List<string> BCCAddresses { get; set; }
        public List<ReplyToAddress> ReplyToAddresses { get; set; }



        //methods
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent
            , List<Subscriber<TKey>> subscribers, List<TemplateData> languageTemplateData)
        {
            List<string> subjects = FillTemplateProperty(SubjectProvider, SubjectTransformer, subscribers, languageTemplateData);
            List<string> bodies = FillTemplateProperty(BodyProvider, BodyTransformer, subscribers, languageTemplateData);

            return subscribers
                .Select((subscriber, i) => AssembleEmail(settings, signalEvent, subscriber, subjects[i], bodies[i]))
                .Cast<SignalDispatch<TKey>>()
                .ToList();
        }

        protected virtual EmailDispatch<TKey> AssembleEmail(EventSettings<TKey> settings,
            SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber, string subject, string body)
        {
            var dispatch = new EmailDispatch<TKey>()
            {
                ReceiverDisplayName = subscriber.Address,
                MessageSubject = subject,
                MessageBody = body,
                IsBodyHtml = IsBodyHtml,
                CCAddresses = CCAddresses,
                BCCAddresses = BCCAddresses,
                ReplyToAddresses = ReplyToAddresses
            };

            SetBaseProperties(dispatch, settings, signalEvent, subscriber);
            return dispatch;
        }
    }
}
