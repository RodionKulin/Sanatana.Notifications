using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;

namespace SignaloBot.Sender.Senders.LimitManager
{
    [Serializable]
    public class LimitedPeriod
    {
        int _limit;


        //свойства
        public TimeSpan Period { get; set; }

        public int Limit
        {
            get { return _limit; }
            set
            {
                if (_limit < 0)
                    throw new ArgumentOutOfRangeException("Лимит не может быть меньше 0.");

                _limit = value;
            }
        }


        //инициализация
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
