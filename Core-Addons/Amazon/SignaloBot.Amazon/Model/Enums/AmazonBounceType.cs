using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.Enums
{
    public enum AmazonBounceType
    {
        /// <summary>
        /// Amazon SES was unable to determine a specific bounce reason.
        /// </summary>
        Undetermined,
        /// <summary>
        /// Amazon SES received a general hard bounce and recommends that you remove the recipient's email address from your mailing list.
        /// </summary>
        Permanent,
        /// <summary>
        /// Amazon SES received a general bounce. You may be able to successfully retry sending to that recipient in the future.
        /// </summary>
        Transient,
        /// <summary>
        /// Unknown type
        /// </summary>
        Unknown
    }
}
