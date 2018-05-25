using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sanatana.Notifications.NDR.AWS.SNS
{
    public class Subscription
    {
        //fields
        protected ILogger _logger;


        //init
        public Subscription(ILogger logger)
        {
            _logger = logger;
        }
        


        //subscribe
        public bool ConfirmSubscription(AmazonSnsMessage amazonSnsMessage)
        {
            string subscribeURL = amazonSnsMessage.SubscribeURL;

            Uri confirmUri;
            bool uriIsValid = Uri.TryCreate(subscribeURL, UriKind.Absolute, out confirmUri);
            if (!uriIsValid)
            {
                _logger.LogError($"SNS subscription confirmation url is not valid {subscribeURL}");
                return false;
            }

            string response;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    response = webClient.DownloadString(confirmUri);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return false;
            }

            _logger.LogDebug($"SNS subscription confirmation response resived: {response}");

            bool confirmed = CheckSubscribeResponse(response);
            _logger.LogDebug($"SNS subscription confirmation finished with result: {confirmed}");

            return confirmed;
        }

        private bool CheckSubscribeResponse(string response)
        {
            bool result = false;

            try
            {
                XDocument doc = XDocument.Parse(response);
                string ns = doc.Root.Name.NamespaceName;
                string subscriptionArn;
                string requestId;

                XElement confirmSubscriptionResult = doc.Root.Element(XName.Get("ConfirmSubscriptionResult", ns));
                subscriptionArn = confirmSubscriptionResult.Element(XName.Get("SubscriptionArn", ns)).Value;

                XElement responseMetadata = doc.Root.Element(XName.Get("ResponseMetadata", ns));
                requestId = responseMetadata.Element(XName.Get("RequestId", ns)).Value;

                result = !string.IsNullOrEmpty(subscriptionArn) && !string.IsNullOrEmpty(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when checking SNS subscription confirmation response: {response}");
            }

            return result;
        }



        //unsubsribe
        public bool Unsubscribe(AmazonSnsMessage amazonSnsMessage)
        {
            string unsubscribeURL = amazonSnsMessage.UnsubscribeURL;

            Uri confirmUri;
            bool uriIsValid = Uri.TryCreate(unsubscribeURL, UriKind.Absolute, out confirmUri);
            if (!uriIsValid)
            {
                _logger.LogError($"SNS unsubsribe url is not valid: {unsubscribeURL}");
                return false;
            }

            string response;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    response = webClient.DownloadString(confirmUri);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return false;
            }

            _logger.LogDebug($"SNS unsubscribe response received: {response}");

            return CheckUnsubscribeResponse(response);
        }

        private bool CheckUnsubscribeResponse(string response)
        {
            bool result = false;

            try
            {
                XDocument doc = XDocument.Parse(response);
                string ns = doc.Root.Name.NamespaceName;
                string requestId;

                XElement responseMetadata = doc.Root.Element(XName.Get("ResponseMetadata", ns));
                requestId = responseMetadata.Element(XName.Get("RequestId", ns)).Value;

                result = !string.IsNullOrEmpty(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when checking SNS unsubscription confirmation response: {response}");
            }

            return result;
        }
    }
}
