using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Client.DelayScheduler
{
    public interface IDelayScheduler
    {
        DateTime GetSendTime(string timezoneID, List<UserReceivePeriod> periods, out bool isDelayed);
    }
}
