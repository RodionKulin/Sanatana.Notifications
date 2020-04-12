using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.Flushing
{
    public enum FlushAction
    {
        Insert,
        Update,
        DeleteOne,
        DeleteConsolidated
    }
}
