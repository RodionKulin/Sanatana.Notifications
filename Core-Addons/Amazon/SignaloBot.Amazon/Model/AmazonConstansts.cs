using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon
{
    internal static class AmazonConstansts
    {
        //AmazonLimitManager
        /// <summary>
        /// Время между обновлением сведений о текущей квоте на рассылку.
        /// </summary>
        public static readonly TimeSpan LIMITMANAGER_QUOTA_REQUEST_PERIOD = TimeSpan.FromHours(12);
        /// <summary>
        /// Пауза после неудавшегося запроса квоты на рассылку.
        /// </summary>
        public static readonly TimeSpan LIMITMANAGER_FAILED_QUOTA_REQUEST_RETRY_PERIOD = TimeSpan.FromSeconds(30);


        //Bounce details xml fields
        public const string DETAILS_NDR_TYPE = "NDRType";
        public const string DETAILS_BOUNCE_TYPE = "BounceType";
        public const string DETAILS_BOUNCE_SUB_TYPE = "BounceSubType";
        public const string DETAILS_COMPLAINT_FEEDBACK_TYPE = "ComplaintFeedbackType";


        //AmazonCredentials
        public const string UNKNOWN_CREDENTIALS_REGION = "Unknown";


        //SNS SignatureVerification
        public const string SNS_SUPPORTED_SIGNATURE_VERSION = "1";
        public const string SNS_SIGNING_CERTIFICATE_URL_END = ".amazonaws.com";
    }
}
