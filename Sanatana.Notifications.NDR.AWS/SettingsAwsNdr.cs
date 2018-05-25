using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Notifications.NDR.AWS
{
    public class SettingsAwsNdr
    {
        /// <summary>
        /// Accept subsciption on receiving SNS messages on this address.
        /// </summary>
        public bool ConfirmSubsription { get; set; } = true;

        /// <summary>
        /// Source email addresses. If specified, then only NDR with contained source will be handled.
        /// </summary>
        public List<string> SourceAddressesToVerify { get; set; }


    }
}
