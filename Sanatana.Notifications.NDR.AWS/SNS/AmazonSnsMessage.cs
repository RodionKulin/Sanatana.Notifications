using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Sanatana.Notifications.NDR.AWS.SNS
{
    public class AmazonSnsMessage
    {
        //properties
        public string Type { get; set; }
        public Guid MessageId { get; set; }
        public string TopicArn { get; set; }

        public string Token { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string SubscribeURL { get; set; }

        public string SignatureVersion { get; set; }
        public string Signature { get; set; }
        public string SigningCertURL { get; set; }
        public string UnsubscribeURL { get; set; }


        //dependent properties
        public AmazonSnsMessageType AmazonSnsMessageType
        {
            get
            {
                if (Type == "SubscriptionConfirmation")
                    return AmazonSnsMessageType.SubscriptionConfirmation;
                if (Type == "UnsubscribeConfirmation")
                    return AmazonSnsMessageType.UnsubscribeConfirmation;
                else if (Type == "Notification")
                    return AmazonSnsMessageType.Notification;
                else
                    return AmazonSnsMessageType.Unknown;
            }
        }
        


        //init
        public static bool TryCreate(string jsonMessage, out AmazonSnsMessage amazonSnsMessage)
        {
            amazonSnsMessage = null;
            bool result = false;

            if (string.IsNullOrEmpty(jsonMessage))
                return false;

            try
            {
                using (TextReader reader = new StringReader(jsonMessage))
                using (var jsonRreader = new Newtonsoft.Json.JsonTextReader(reader))
                {
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    amazonSnsMessage = serializer.Deserialize<AmazonSnsMessage>(jsonRreader);
                }

                result = true;
            }
            catch (Exception)
            {
            }

            return result;       
        }



        //methods
        public string GenerateContentString()
        {
            StringBuilder fullMessageSb = new StringBuilder();

            if (AmazonSnsMessageType == AmazonSnsMessageType.Notification)
            {
                fullMessageSb.Append("Message").Append("\n");
                fullMessageSb.Append(Message).Append("\n");
                fullMessageSb.Append("MessageId").Append("\n");
                fullMessageSb.Append(MessageId.ToString()).Append("\n");

                if (Subject != null)
                {
                    fullMessageSb.Append("Subject").Append("\n");
                    fullMessageSb.Append(Subject).Append("\n");
                }
                
                fullMessageSb.Append("Timestamp").Append("\n");
                fullMessageSb.Append(Timestamp).Append("\n");
                fullMessageSb.Append("TopicArn").Append("\n");
                fullMessageSb.Append(TopicArn).Append("\n");
                fullMessageSb.Append("Type").Append("\n");
                fullMessageSb.Append(Type).Append("\n");
            }
            else if (AmazonSnsMessageType == AmazonSnsMessageType.SubscriptionConfirmation ||
                AmazonSnsMessageType == AmazonSnsMessageType.UnsubscribeConfirmation)
            {
                fullMessageSb.Append("Message").Append("\n");
                fullMessageSb.Append(Message).Append("\n");
                fullMessageSb.Append("MessageId").Append("\n");
                fullMessageSb.Append(MessageId.ToString()).Append("\n");
                fullMessageSb.Append("SubscribeURL").Append("\n");
                fullMessageSb.Append(SubscribeURL).Append("\n");
                fullMessageSb.Append("Timestamp").Append("\n");
                fullMessageSb.Append(Timestamp).Append("\n");
                fullMessageSb.Append("Token").Append("\n");
                fullMessageSb.Append(Token).Append("\n");
                fullMessageSb.Append("TopicArn").Append("\n");
                fullMessageSb.Append(TopicArn).Append("\n");
                fullMessageSb.Append("Type").Append("\n");
                fullMessageSb.Append(Type).Append("\n");
            }

            return fullMessageSb.ToString();
        }
    }
}