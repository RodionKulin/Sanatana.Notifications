using Sanatana.Notifications.Composing.Templates;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sanatana.Notifications.DeliveryTypes.Slack
{
    public class SlackDispatchTemplate<TKey> : DispatchTemplate<TKey>
        where TKey : struct
    {
        //properties  
        /// <summary>
        /// Channel to send message to
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// Username to send message to
        /// </summary>
        public string Username { get; set; }
        public ITemplateProvider TextProvider { get; set; }
        public ITemplateTransformer TextTransformer { get; set; }



        //methods
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, List<Subscriber<TKey>> subscribers)
        {
            TemplateData templateData = new TemplateData(signalEvent.DataKeyValues);
            var templateDataList = new List<TemplateData>() { templateData };

            string content = null;
            if (TextProvider != null && TextTransformer != null)
            {
                content = TextTransformer.Transform(TextProvider, templateDataList)
                    .First();
            }

            var list = new List<SignalDispatch<TKey>>();
            for (int i = 0; i < subscribers.Count; i++)
            {
                SlackDispatch<TKey> dispatch = Build(settings, signalEvent, subscribers[i], content);
                list.Add(dispatch);
            }

            return list;
        }

        protected virtual SlackDispatch<TKey> Build(EventSettings<TKey> settings
            , SignalEvent<TKey> signalEvent, Subscriber<TKey> subscriber, string content)
        {
            var dispatch = new SlackDispatch<TKey>()
            {
                Text = content,
                Channel = Channel,
                Username = Username
            };

            SetBaseProperties(dispatch, settings, signalEvent, subscriber);
            return dispatch;
        }
    }
}
