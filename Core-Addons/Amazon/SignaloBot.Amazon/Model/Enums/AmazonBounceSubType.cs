using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.Enums
{
    public enum AmazonBounceSubType
    {
        /// <summary>
        /// Amazon SES was unable to determine a specific bounce reason.
        /// </summary>
        Undetermined,
        /// <summary>
        /// Permanent - Amazon SES received a general hard bounce and recommends that you remove the recipient's email address from your mailing list.
        /// Transient - Amazon SES received a general bounce. You may be able to successfully retry sending to that recipient in the future.
        /// </summary>
        General,
        /// <summary>
        /// Amazon SES received a permanent hard bounce because the target email address does not exist. It is recommended that you remove that recipient from your mailing list.
        /// </summary>
        NoEmail,
        /// <summary>
        /// Amazon SES has suppressed sending to this address because it has a recent history of bouncing as an invalid address. For information about how to remove an address from the suppression list, see Removing an Email Address from the Amazon SES Suppression List.
        /// </summary>
        Suppressed,
        /// <summary>
        /// Amazon SES received a mailbox full bounce. You may be able to successfully retry sending to that recipient in the future.
        /// </summary>
        MailboxFull,
        /// <summary>
        /// Amazon SES received a message too large bounce. You may be able to successfully retry sending to that recipient if you reduce the message size.
        /// </summary>
        MessageTooLarge,
        /// <summary>
        /// Amazon SES received a content rejected bounce. You may be able to successfully retry sending to that recipient if you change the message content.
        /// </summary>
        ContentRejected,
        /// <summary>
        /// Amazon SES received an attachment rejected bounce. You may be able to successfully retry sending to that recipient if you remove or change the attachment.
        /// </summary>
        AttachmentRejected,
        /// <summary>
        /// Unknown type
        /// </summary>
        Unknown
    }
}
