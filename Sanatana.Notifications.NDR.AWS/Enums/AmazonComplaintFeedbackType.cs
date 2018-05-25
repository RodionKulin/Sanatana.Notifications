using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS
{
    public enum AmazonComplaintFeedbackType
    {
        /// <summary>
        /// Indicates unsolicited email or some other kind of email abuse.
        /// </summary>
        Abuse,
        /// <summary>
        /// Indicates unsolicited email or some other kind of email abuse.
        /// </summary>
        AuthFailure,
        /// <summary>
        /// Indicates some kind of fraud or phishing activity.
        /// </summary>
        Fraud,
        /// <summary>
        /// Indicates that the entity providing the report does not consider the message to be spam. This may be used to correct a message that was incorrectly tagged or categorized as spam.
        /// </summary>
        NotSpam,
        /// <summary>
        /// Indicates any other feedback that does not fit into other registered types.
        /// </summary>
        Other,
        /// <summary>
        /// Reports that a virus is found in the originating message.
        /// </summary>
        Virus,
        /// <summary>
        /// Unknown type
        /// </summary>
        Unknown
    }
}
