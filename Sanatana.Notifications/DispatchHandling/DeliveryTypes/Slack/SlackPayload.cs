using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DispatchHandling.DeliveryTypes.Slack
{
    public class SlackPayload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
