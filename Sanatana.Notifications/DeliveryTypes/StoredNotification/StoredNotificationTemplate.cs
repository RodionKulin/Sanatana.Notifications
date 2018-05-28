using Sanatana.Notifications.Composing;
using Sanatana.Notifications.Composing.Templates;
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
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            TemplateData templateData = new TemplateData(signalEvent.DataKeyValues);
            var templateDataList = new List<TemplateData>() { templateData };

            string subject = null;
            if (SubjectProvider != null && SubjectTransformer != null)
            {
                subject = SubjectTransformer.Transform(SubjectProvider, templateDataList)
                    .First();
            }

            string body = null;
            if (BodyProvider != null && BodyTransformer != null)
            {
                body = BodyTransformer.Transform(BodyProvider, templateDataList)
                    .First();
            }

            var list = new List<SignalDispatch<TKey>>();
            for (int i = 0; i < subscribers.Count; i++)
            {
                StoredNotificationDispatch<TKey> dispatch = Build(settings, signalEvent, subscribers[i], subject, body);
                list.Add(dispatch);
            }

            return list;
        }

        protected virtual StoredNotificationDispatch<TKey> Build(EventSettings<TKey> settings
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
    }
}
