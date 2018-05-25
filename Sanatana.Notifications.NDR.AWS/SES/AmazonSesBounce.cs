using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS.SES
{
    public class AmazonSesBounce
    {
        //properties
        public string BounceType { get; set; }
        public string ReportingMTA { get; set; }
        public string BounceSubType { get; set; }
        public List<AmazonSesBouncedRecipient> BouncedRecipients { get; set; }
        public string Timestamp { get; set; }
        public string FeedbackId { get; set; }

        //зависимые properties
        public AmazonBounceType AmazonBounceType
        {
            get
            {
                if (BounceType == "Undetermined")
                    return AmazonBounceType.Undetermined;
                else if (BounceType == "Permanent")
                    return AmazonBounceType.Permanent;
                else if (BounceType == "Transient")
                    return AmazonBounceType.Transient;
                else
                    return AmazonBounceType.Unknown;
            }
        }
        public AmazonBounceSubType AmazonBounceSubType
        {
            get
            {
                if (BounceSubType == "Undetermined")
                    return AmazonBounceSubType.Undetermined;
                else if (BounceSubType == "General")
                    return AmazonBounceSubType.General;
                else if (BounceSubType == "NoEmail")
                    return AmazonBounceSubType.NoEmail;
                else if (BounceSubType == "Suppressed")
                    return AmazonBounceSubType.Suppressed;
                else if (BounceSubType == "MailboxFull")
                    return AmazonBounceSubType.MailboxFull;
                else if (BounceSubType == "MessageToolarge")
                    return AmazonBounceSubType.MessageTooLarge;
                else if (BounceSubType == "ContentRejected")
                    return AmazonBounceSubType.ContentRejected;
                else if (BounceSubType == "AttachmentRejected")
                    return AmazonBounceSubType.AttachmentRejected;
                else
                    return AmazonBounceSubType.Unknown;
            }
        }
    }
}
