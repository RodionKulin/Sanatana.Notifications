using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace Sanatana.Notifications.NDR.AWS.SNS
{
    public class AmazonSnsManager
    {
        //fields
        protected bool _confirmSubsription = true;
        protected ILogger _logger;
        protected SignatureVerification _signatureVerification;
        protected Subscription _subscription;


        // properties
        /// <summary>
        /// Accept subsciption on receiving SNS messages on this address.
        /// </summary>
        public bool ConfirmSubsription
        {
            get { return _confirmSubsription; }
            set { _confirmSubsription = value; }
        }


        // init
        public AmazonSnsManager(ILogger logger)
        {
            _logger = logger;
            _signatureVerification = new SignatureVerification(logger);
            _subscription = new Subscription(logger);
        }



        // parse request message
        public bool ParseRequest(Stream request, out AmazonSnsMessage message)
        {
            string requestMessage = null;

            try
            {
                using (StreamReader reader = new StreamReader(request))
                {
                    requestMessage = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }

            if (requestMessage == null)
            {
                message = null;
                return false;
            }
            else
            {
                return ParseRequest(requestMessage, out message);
            }
        }

        public bool ParseRequest(string request, out AmazonSnsMessage message)
        {
            message = null;
            bool isValid = false;

            // parse json
            AmazonSnsMessage amazonSnsMessage;
            bool created = AmazonSnsMessage.TryCreate(request, out amazonSnsMessage);
            if (!created)
            {
                _logger.LogError($"SNS message was not successfuly parsed: {request}");
                return false;
            }

            // verify signature
            bool verified = _signatureVerification.VerifySignature(amazonSnsMessage);
            if (!verified)
            {
                _logger.LogError($"SNS Signature verification failed: {request}");
                return false;
            }

            
            // handle depending on type
            if (amazonSnsMessage.AmazonSnsMessageType == AmazonSnsMessageType.Notification)
            {
                message = amazonSnsMessage;
                isValid = message.Message != null;
            }
            // subscribe
            else if (amazonSnsMessage.AmazonSnsMessageType == AmazonSnsMessageType.SubscriptionConfirmation
                && ConfirmSubsription)
            {
                isValid = _subscription.ConfirmSubscription(amazonSnsMessage);
            }
            // unknown type
            else
            {
                _logger.LogError($"SNS Unknown message type received: {request}");
                isValid = false;
            }

            return isValid;
        }
    }
}