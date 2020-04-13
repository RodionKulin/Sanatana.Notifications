using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sanatana.Notifications.DAL.Entities
{
    public class SubscriberScheduleSettings<TKey>
        where TKey : struct
    {
        public TKey SubscriberScheduleSettingsId { get; set; }
        public TKey SubscriberId { get; set; }
        public int Set { get; set; }
        public int Order { get; set; }
        public TimeSpan PeriodBegin { get; set; }
        public TimeSpan PeriodEnd { get; set; }
    }
}