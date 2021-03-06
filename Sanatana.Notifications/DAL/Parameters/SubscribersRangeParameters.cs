﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Parameters
{ 
    public class SubscribersRangeParameters<TKey>
        where TKey : struct
    {
        public List<TKey> FromSubscriberIds { get; set; }
        public TKey? SubscriberIdRangeFromIncludingSelf { get; set; }
        public TKey? SubscriberIdRangeToIncludingSelf { get; set; }
        public List<int> SubscriberIdFromDeliveryTypesHandled { get; set; }
        public int? Limit { get; set; }
        public string TopicId { get; set; }
        public Dictionary<string,string> SubscriberFilters { get; set; }
        public virtual bool SelectFromCategories { get; set; }
        public virtual bool SelectFromTopics { get; set; }
    }
}
