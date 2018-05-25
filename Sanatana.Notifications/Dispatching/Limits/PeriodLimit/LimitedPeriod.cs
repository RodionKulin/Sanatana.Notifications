using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Resources;

namespace Sanatana.Notifications.Dispatching.Limits
{
    [Serializable]
    public class LimitedPeriod
    {
        protected int _limit;


        //properties
        public TimeSpan Period { get; set; }
        public virtual int Limit
        {
            get { return _limit; }
            set
            {
                if (_limit < 0)
                {
                    throw new ArgumentOutOfRangeException(SenderInternalMessages.LimitPeriod_LowerBound);
                }

                _limit = value;
            }
        }


        //init
        public LimitedPeriod()
        {
            Period = TimeSpan.Zero;
            Limit = 0;
        }

        public LimitedPeriod(TimeSpan period, int limit)
        {
            Period = period;
            Limit = limit;
        }
    }
}
