using SignaloBot.Amazon.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SignaloBot.Amazon.NDR
{
    internal class XmlBounceDetails
    {
        public static string DetailsToXml(AmazonSesMessageType type, AmazonBounceType? bounceType = null
           , AmazonBounceSubType? bounceSubType = null, AmazonComplaintFeedbackType? complaintFeedbackType = null)
        {
            XDocument doc = new XDocument();

            doc.Root.Add(new XElement(AmazonConstansts.DETAILS_NDR_TYPE, type));

            if (bounceType != null)
            {
                doc.Root.Add(new XElement(AmazonConstansts.DETAILS_BOUNCE_TYPE, bounceType.Value));
            }

            if (bounceSubType != null)
            {
                doc.Root.Add(new XElement(AmazonConstansts.DETAILS_BOUNCE_SUB_TYPE, bounceSubType.Value));
            }
            if (complaintFeedbackType != null)
            {
                doc.Root.Add(new XElement(AmazonConstansts.DETAILS_COMPLAINT_FEEDBACK_TYPE, complaintFeedbackType.Value));
            }

            StringBuilder xmlString = new StringBuilder();
            using (StringWriter sw = new StringWriter(xmlString))
            {
                doc.Save(sw);
            }
            return xmlString.ToString();
        }

    }
}
