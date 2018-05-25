using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS
{
    public static class AmazonConstansts
    {
        //Bounce details xml fields
        public const string DETAILS_NDR_TYPE = "NDRType";
        public const string DETAILS_BOUNCE_TYPE = "BounceType";
        public const string DETAILS_BOUNCE_SUB_TYPE = "BounceSubType";
        public const string DETAILS_COMPLAINT_FEEDBACK_TYPE = "ComplaintFeedbackType";
        
        //SNS SignatureVerification
        public const string SNS_SUPPORTED_SIGNATURE_VERSION = "1";
        public const string SNS_SIGNING_CERTIFICATE_URL_END = ".amazonaws.com";
    }
}
