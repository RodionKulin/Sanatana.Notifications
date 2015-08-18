using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    internal class DALConstants
    {
        /// <summary>
        /// Максимальная длина адреса, согласно http://stackoverflow.com/questions/386294/what-is-the-maximum-length-of-a-valid-email-address
        /// </summary>
        public const int EMAIL_MAX_ADDRESS_LENGTH = 254;
        public const int EMAIL_MAX_SUBJECT_LENGTH = 200;
        public const int EMAIL_MAX_SENDER_NAME_LENGTH = 200;
        public const int EMAIL_MAX_CONTENT_LENGTH = 20000000;
    }
}
