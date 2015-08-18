using SignaloBot.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.NDR.Model
{
    public interface INDRParser
    {
        List<BouncedMessage> ParseBounceInfo(string requestMessage);
    }
}
