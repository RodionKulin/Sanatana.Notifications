using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.NDR.AWS.SNS;
using Sanatana.Notifications.NDR.AWS.SES;
using System.Xml.Linq;
using Sanatana.Notifications.DAL.Entities;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.NDR.AWS
{
    public class AmazonNDRParser<TKey> : INdrParser<TKey>
        where TKey : struct
    {
        //fields
        protected AmazonSnsManager _amazonSnsManager;
        protected AmazonSesManager _amazonSesManager;
        protected ILogger _logger;


        //properties
        /// <summary>
        /// Source email address. If specified, then only NDR with matching source will be handled.
        /// </summary>
        public string SourceAddressToVerify { get; set; }


        //init
        public AmazonNDRParser(ILogger logger)
        {
            _logger = logger;
            _amazonSnsManager = new AmazonSnsManager(_logger);
            _amazonSesManager = new AmazonSesManager(_logger);
        }



        //methods
        /// <summary>
        /// Handle request from AWS SNS with NDR.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public List<SignalBounce<TKey>> ParseBounceInfo(string requestMessage)
        {
            //string > sns message
            AmazonSnsMessage message;
            bool isValid = _amazonSnsManager.ParseRequest(requestMessage, out message);
            if (isValid == false)
                return new List<SignalBounce<TKey>>();
           
            //sns message > ses message
            AmazonSesNotification sesNotification = ExtractSesNotification(message);
            if (sesNotification == null)
                return new List<SignalBounce<TKey>>();

            //ses message > List<SignalBounce<TKey>>
            return CreateBounceList(sesNotification);
        }

        private AmazonSesNotification ExtractSesNotification(AmazonSnsMessage message)
        {
            AmazonSesNotification sesNotification;
            bool isValid = _amazonSesManager.ParseRequest(message.Message, out sesNotification);
            if (isValid == false)            
                return null;

            //check email source address
            if (string.IsNullOrEmpty(SourceAddressToVerify) == false
                && sesNotification.Mail.Source != SourceAddressToVerify)
            {
                string error = $"NDR received for unexpected source address {sesNotification.Mail.Source}, when expecting {SourceAddressToVerify}.";
                _logger.LogError(error);
                return null;
            }

            return sesNotification;
        }               

        private List<SignalBounce<TKey>> CreateBounceList(AmazonSesNotification sesNotification)
        {
            List<SignalBounce<TKey>> bouncedMessages = new List<SignalBounce<TKey>>();
           
            if (sesNotification.AmazonSesMessageType == AmazonSesMessageType.Unknown)
            {
                _logger.LogError("Unknown AWS message type received.");
                return bouncedMessages;
            }
            //parse NDR
            else if (sesNotification.AmazonSesMessageType == AmazonSesMessageType.Bounce)
            {
                foreach (AmazonSesBouncedRecipient recipient in sesNotification.Bounce.BouncedRecipients)
                {
                    BounceType bounceType = sesNotification.Bounce.AmazonBounceType == AmazonBounceType.Permanent
                            ? BounceType.HardBounce
                            : BounceType.SoftBounce;

                    string detailsXml = XmlBounceDetails.DetailsToXml(sesNotification.AmazonSesMessageType
                        , sesNotification.Bounce.AmazonBounceType, sesNotification.Bounce.AmazonBounceSubType);
                                          
                    SignalBounce<TKey> bouncedMessage = CreateBouncedMessage(bounceType
                          , sesNotification.Mail, recipient.EmailAddress, detailsXml);

                    bouncedMessages.Add(bouncedMessage);
                }
            }
            //parse complaint
            else if (sesNotification.AmazonSesMessageType == AmazonSesMessageType.Complaint)
            {
                foreach (AmazonSesComplaintRecipient recipient in sesNotification.Complaint.ComplainedRecipients)
                {
                    string detailsXml = XmlBounceDetails.DetailsToXml(sesNotification.AmazonSesMessageType
                        , complaintFeedbackType: sesNotification.Complaint.AmazonComplaintFeedbackType);

                    SignalBounce<TKey> bouncedMessage = CreateBouncedMessage(BounceType.HardBounce
                        , sesNotification.Mail, recipient.EmailAddress, detailsXml);

                    bouncedMessages.Add(bouncedMessage);
                }
            }

            return bouncedMessages;
        }

        private SignalBounce<TKey> CreateBouncedMessage(BounceType type, AmazonSesMail mail
            , string recipientEmail, string detailsXml)
        {
            SignalBounce<TKey> bouncedMessage = new SignalBounce<TKey>()
            {
                ReceiverAddress = recipientEmail,

                BounceType = type,

                BounceReceiveDateUtc = DateTime.UtcNow,
                BounceDetailsXML = detailsXml,

                MessageBody = null,
                MessageSubject = null,
                SendDateUtc = null 

                //DeliveryType, ReceiverSubscriberID, BouncedMessageID
                //are set by NdrHandler
            };


            DateTime timestamp;
            if(DateTime.TryParse(mail.Timestamp, out timestamp))
            {
                bouncedMessage.SendDateUtc = timestamp;
            }

            return bouncedMessage;
        }
    }
}
