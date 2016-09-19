using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class SubscribersIntermidiateResult<TKey>
        where TKey : struct
    {
        public List<UserCategorySettings<TKey>> CategorySubscribers { get; set; }
        public List<UserTopicSettings<TKey>> TopicSubscribers { get; set; }
    }
}
