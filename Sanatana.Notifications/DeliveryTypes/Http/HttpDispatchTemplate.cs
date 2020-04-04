using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.Notifications.EventsHandling.Templates;
using System.Net.Http;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;

namespace Sanatana.Notifications.DeliveryTypes.Http
{
    public class HttpDispatchTemplate<TKey> : DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties  
        public string Url { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public ITemplateProvider ContentProvider { get; set; }
        public ITemplateTransformer ContentTransformer { get; set; }



        //methods
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent,
             List<Subscriber<TKey>> subscribers, List<TemplateData> cultureAndData)
        {
            List<string> bodies = FillTemplates(ContentProvider, ContentTransformer, subscribers, cultureAndData);
            
            return subscribers
                .Select((subscriber, i) => AssembleHttpRequest(settings, signalEvent, subscriber, bodies[i]))
                .Cast<SignalDispatch<TKey>>()
                .ToList();
        }

        protected virtual HttpDispatch<TKey> AssembleHttpRequest(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber, string content)
        {
            var dispatch = new HttpDispatch<TKey>()
            {
                Url = Url,
                HttpMethod = HttpMethod,
                Headers = Headers,
                Content = content
            };

            SetBaseProperties(dispatch, settings, signalEvent, subscriber);
            return dispatch;
        }
    }
}
