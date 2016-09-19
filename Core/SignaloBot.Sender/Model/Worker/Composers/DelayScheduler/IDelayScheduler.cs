using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Composers
{
    public interface IDelayScheduler<TKey>
        where TKey : struct
    {
        DateTime GetSendTime(string timezoneID, List<UserReceivePeriod<TKey>> periods, out bool isDelayed);
    }
}
