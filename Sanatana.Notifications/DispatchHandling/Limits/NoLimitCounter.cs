using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling.Limits
{
    public class NoLimitCounter : ILimitCounter
    {
        //methods
        public virtual void InsertTime()
        {
        }

        public virtual DateTime? GetLimitsEndTimeUtc()
        {
            return null;
        }

        public virtual int GetLimitCapacity()
        {
            return int.MaxValue;
        }

        public virtual void Dispose()
        {
        }
    }
}
