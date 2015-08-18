using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL.Entities.Core
{
    public interface IMessage
    {
        int DeliveryType { get; set; }

        DateTime SendDateUtc { get; set; }

        bool IsDelayed { get; set; }

        int FailedAttempts { get; set; }
    }
}
