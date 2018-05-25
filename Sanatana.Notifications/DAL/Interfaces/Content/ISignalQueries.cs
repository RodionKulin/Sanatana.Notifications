using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Interfaces
{
    public interface ISignalQueries<T>
    {
        Task Insert(List<T> items);

        Task UpdateSendResults(List<T> items);

        Task Delete(List<T> items);
    }
}
