using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders.LimitManager
{
    public class NoLimitManager : ILimitManager
    {
        public void InsertTime()
        {
        }

        public DateTime GetLimitsEndTimeUtc()
        {
            return DateTime.UtcNow;
        }

        public int GetLimitCapacity()
        {
            return int.MaxValue;
        }

        public void Dispose()
        {
        }
    }
}
