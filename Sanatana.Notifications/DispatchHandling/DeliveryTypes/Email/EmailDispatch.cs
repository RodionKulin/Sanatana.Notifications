using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling.DeliveryTypes.Email
{
    [Serializable]
    public class EmailDispatch<TKey> : SignalDispatch<TKey>
        where TKey :struct
    {
        public string ReceiverDisplayName { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public bool IsBodyHtml { get; set; }
        public List<string> CCAddresses { get; set; }
        public List<string> BCCAddresses { get; set; }
        public List<ReplyToAddress> ReplyToAddresses { get; set; }
    }
}
