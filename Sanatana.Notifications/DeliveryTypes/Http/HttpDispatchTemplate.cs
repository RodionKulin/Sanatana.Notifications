using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using System.Net.Mail;
using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.Composing;
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
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            TemplateData templateData = new TemplateData(signalEvent.DataKeyValues);
            var templateDataList = new List<TemplateData>() { templateData };

            string content = null;
            if (ContentProvider != null && ContentTransformer != null)
            {
                content = ContentTransformer.Transform(ContentProvider, templateDataList)
                    .First();
            }

            var list = new List<SignalDispatch<TKey>>();
            for (int i = 0; i < subscribers.Count; i++)
            {
                HttpDispatch<TKey> dispatch = Build(settings, signalEvent, subscribers[i], content);
                list.Add(dispatch);
            }

            return list;
        }

        protected virtual HttpDispatch<TKey> Build(EventSettings<TKey> settings
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
