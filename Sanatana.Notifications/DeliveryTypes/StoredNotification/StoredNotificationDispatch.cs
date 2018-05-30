using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DeliveryTypes.StoredNotification
{
    [Serializable]
    public class StoredNotificationDispatch<TKey> : SignalDispatch<TKey>
        where TKey : struct
    {
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
    }
}
