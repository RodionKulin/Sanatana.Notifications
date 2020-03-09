using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using System.Net.Mail;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.Composing;
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
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            TemplateData templateData = new TemplateData(signalEvent.TemplateData);
            var templateDataList = new List<TemplateData>() { templateData };

            string body = null;
            if (BodyProvider != null && BodyTransformer != null)
            {
                body = BodyTransformer.Transform(BodyProvider, templateDataList)
                    .First();
            }

            string subject = null;
            if (SubjectProvider != null && SubjectTransformer != null)
            {
                subject = SubjectTransformer.Transform(SubjectProvider, templateDataList)
                    .First();
            }

            var list = new List<SignalDispatch<TKey>>();
            for (int i = 0; i < subscribers.Count; i++)
            {
                EmailDispatch<TKey> dispatch = Build(settings, signalEvent, subscribers[i], subject, body);
                list.Add(dispatch);
            }

            return list;
        }

        protected virtual EmailDispatch<TKey> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber, string subject, string body)
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
