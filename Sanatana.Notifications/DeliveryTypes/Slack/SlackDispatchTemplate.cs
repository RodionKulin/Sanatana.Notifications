using Sanatana.Notifications.EventsHandling.Templates;
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
        public override List<SignalDispatch<TKey>> Build(EventSettings<TKey> settings, SignalEvent<TKey> signalEvent,
             List<Subscriber<TKey>> subscribers, List<TemplateData> languageTemplateData)
        {
            List<string> texts = FillTemplateProperty(TextProvider, TextTransformer, subscribers, languageTemplateData);

            return subscribers
                .Select((subscriber, i) => AssembleSlackMessage(settings, signalEvent, subscriber, texts[i]))
                .Cast<SignalDispatch<TKey>>()
                .ToList();
        }

        protected virtual SlackDispatch<TKey> AssembleSlackMessage(EventSettings<TKey> settings
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

        public override void Update(SignalDispatch<TKey> item, TemplateData templateData)
        {
            var dispatch = (SlackDispatch<TKey>)item;
            dispatch.Text = FillTemplateProperty(TextProvider, TextTransformer, templateData);
        }
    }
}
