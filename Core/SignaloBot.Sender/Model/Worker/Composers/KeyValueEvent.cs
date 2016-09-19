using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SignaloBot.Sender
{
    public class KeyValueEvent<TKey> : SignalEventBase<TKey>
        where TKey : struct
    {
        public virtual Dictionary<string, string> Values { get; set; }
    }
}