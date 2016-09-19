using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders.Email
{
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public NetworkCredential Credentials { get; set; }
    }
}
