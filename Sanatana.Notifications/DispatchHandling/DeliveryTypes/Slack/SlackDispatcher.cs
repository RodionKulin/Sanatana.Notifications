using Newtonsoft.Json;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.Processing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling.DeliveryTypes.Slack
{
    public class SlackDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //fields
        protected Uri _urlWithAccessToken;


        //init
        protected SlackDispatcher()
        {
        }
        public SlackDispatcher(Uri urlWithAccessToken)
        {
            _urlWithAccessToken = urlWithAccessToken;
        }


        //methods
        public virtual async Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            SlackDispatch<TKey> slackDispatch = (SlackDispatch<TKey>)item;

            //serialize payload
            var payload = new SlackPayload()
            {
                Channel = slackDispatch.Channel,
                Username = slackDispatch.Username,
                Text = slackDispatch.Text
            };
            string payloadJson = JsonConvert.SerializeObject(payload);
            var data = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("payload", payloadJson)
            };

            //send payload
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync(_urlWithAccessToken, new FormUrlEncodedContent(data));
                string responseString = await response.Content.ReadAsStringAsync();
               
                bool completed = responseString == "ok";
                if (!completed)
                {
                    return ProcessingResult.Fail;
                }
            }

            return ProcessingResult.Success;
        }

        public virtual Task<DispatcherAvailability> CheckAvailability()
        {
            return Task.FromResult(DispatcherAvailability.Available);
        }

        public virtual void Dispose()
        {
        }

    }
}
