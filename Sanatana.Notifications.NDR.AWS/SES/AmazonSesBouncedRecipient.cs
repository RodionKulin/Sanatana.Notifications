using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS.SES
{
    public class AmazonSesBouncedRecipient
    {
        //properties
        public string EmailAddress { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public string DiagnosticCode { get; set; }
    }
}
