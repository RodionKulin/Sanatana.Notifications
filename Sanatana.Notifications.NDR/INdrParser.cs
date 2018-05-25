using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR
{
    public interface INdrParser<TKey>
        where TKey : struct
    {
        List<SignalBounce<TKey>> ParseBounceInfo(string requestMessage);
    }
}
