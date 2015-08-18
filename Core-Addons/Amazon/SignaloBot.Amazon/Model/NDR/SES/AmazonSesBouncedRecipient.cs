using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.NDR.SES
{
    internal class AmazonSesBouncedRecipient
    {
        //свойства
        public string EmailAddress { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public string DiagnosticCode { get; set; }
    }
}
