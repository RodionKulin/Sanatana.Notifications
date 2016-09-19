using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.NDR.Model
{
    public interface INDRParser<TKey>
        where TKey : struct
    {
        List<SignalBounce<TKey>> ParseBounceInfo(string requestMessage);
    }
}
