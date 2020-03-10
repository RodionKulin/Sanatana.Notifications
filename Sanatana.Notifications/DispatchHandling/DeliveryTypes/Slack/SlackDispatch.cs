using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DispatchHandling.DeliveryTypes.Slack
{
    public class SlackDispatch<TKey> : SignalDispatch<TKey>
        where TKey : struct
    {
        public string Channel { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
    }
}
