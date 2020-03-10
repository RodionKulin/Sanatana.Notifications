using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling.DeliveryTypes.Email
{
    public class SmtpSettings
    {
        /// <summary>
        /// Host of SMTP server that will be forwarding email to receiver
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// SMTP server port
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// SMTP host secure connection usage
        /// </summary>
        public bool EnableSsl { get; set; }
        /// <summary>
        /// Login and password used to authenticate in SMTP server
        /// </summary>
        public NetworkCredential Credentials { get; set; }
        /// <summary>
        /// Required email address that will be shown to receiver in from field. Usually same as credentials login.
        /// </summary>
        public string SenderAddress { get; set; }
        /// <summary>
        /// Optional display name from your email that will send email.
        /// </summary>
        public string SenderDisplayName { get; set; }
    }
}
