using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.DAL.Parameters
{
    public class DispatchQueryParameters<TKey>
        where TKey : struct
    {
        public int Count { get; set; }
        public List<int> ActiveDeliveryTypes { get; set; }
        public int MaxFailedAttempts { get; set; }
        public TKey[] ExcludeIds { get; set; }
        public ConsolidationLock<TKey>[] ExcludeConsolidated { get; set; }
    }
}
