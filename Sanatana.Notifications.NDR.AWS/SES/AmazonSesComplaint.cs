using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS.SES
{
    public class AmazonSesComplaint
    {
        //properties
        public string SubscriberAgent { get; set; }
        public List<AmazonSesComplaintRecipient> ComplainedRecipients { get; set; }
        public string ComplaintFeedbackType { get; set; }
        public string ArrivalDate { get; set; }
        public string Timestamp { get; set; }
        public string FeedbackId { get; set; }


        //зависимые properties
        public AmazonComplaintFeedbackType AmazonComplaintFeedbackType
        {
            get
            {
                if (ComplaintFeedbackType == "abuse")
                    return AmazonComplaintFeedbackType.Abuse;
                else if (ComplaintFeedbackType == "auth-failure")
                    return AmazonComplaintFeedbackType.AuthFailure;
                else if (ComplaintFeedbackType == "fraud")
                    return AmazonComplaintFeedbackType.Fraud;
                else if (ComplaintFeedbackType == "not-spam")
                    return AmazonComplaintFeedbackType.NotSpam;
                else if (ComplaintFeedbackType == "other")
                    return AmazonComplaintFeedbackType.Other;
                else if (ComplaintFeedbackType == "virus")
                    return AmazonComplaintFeedbackType.Virus;
                else
                    return AmazonComplaintFeedbackType.Unknown;
            }
        }
    }
}
