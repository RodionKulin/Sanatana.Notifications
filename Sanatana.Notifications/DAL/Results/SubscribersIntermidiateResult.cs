using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Results
{
    public class SubscribersIntermIdiateResult<TKey>
        where TKey : struct
    {
        public List<SubscriberCategorySettings<TKey>> CategorySubscribers { get; set; }
        public List<SubscriberTopicSettings<TKey>> TopicSubscribers { get; set; }
    }
}
