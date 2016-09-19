using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Queue
{
    public enum FlushAction
    {
        Delete,
        Update,
        Skip,
        Insert
    }
}
