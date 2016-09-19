using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    public class SignalEventBase<TKey>
        where TKey : struct
    {
        public virtual TKey SignalEventID { get; set; }

        public virtual DateTime ReceiveDateUtc { get; set; }

        public virtual int CategoryID { get; set; }
        public virtual string TopicID { get; set; }
        public virtual int FailedAttempts { get; set; }
        public virtual bool IsSplitted { get; set; }
        public virtual TKey? ComposerSettingsID { get; set; }
        public virtual TKey? UserIDRangeFrom { get; set; }
        public virtual TKey? UserIDRangeTo { get; set; }
        public virtual TKey? GroupID { get; set; }


        //методы
        public virtual SignalEventBase<TKey> CreateClone()
        {
            return Common.Utility.CloneExtensions.Clone(this);
        }
    }
}
