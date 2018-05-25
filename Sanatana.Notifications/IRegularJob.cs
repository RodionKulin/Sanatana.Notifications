using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications
{
    public interface IRegularJob
    {
        void Tick();
        void Flush();
    }
}
