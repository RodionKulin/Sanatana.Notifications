using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SignaloBot.Amazon.NDR.SNS
{
    internal class Subscription
    {
        ICommonLogger _logger;


        //инициализация
        public Subscription(ICommonLogger logger)
        {
            _logger = logger;
        }
        


        //подписаться
        public bool ConfirmSubscription(AmazonSnsMessage amazonSnsMessage)
        {
            string subscribeURL = amazonSnsMessage.SubscribeURL;

            Uri confirmUri;
            bool uriIsValid = Uri.TryCreate(subscribeURL, UriKind.Absolute, out confirmUri);
            if (!uriIsValid)
            {
                if(_logger != null)
                    _logger.Error("Неверный адрес подтверждения подписки Amazon Sns {0}", subscribeURL);
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
                if (_logger != null)
                    _logger.Exception(ex, "Ошибка при получении ответа на подтверждение подписки Amazon Sns.");
                return false;
            }

            if (_logger != null)
                _logger.Debug("Получен ответ на подтверждение подписки Amazon Sns: {0}", response);

            bool confirmed = CheckSubscribeResponse(response);
            
            if (_logger != null)
                _logger.Info("Подтверждение подписки Amazon Sns закончилось с результатом {0}", confirmed);

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
                if (_logger != null)
                    _logger.Exception(ex, "Ошибка при проверке ответа на подтверждение подписки Amazon Sns: {0}", response);
            }

            return result;
        }



        //отписаться
        public bool Unsubscribe(AmazonSnsMessage amazonSnsMessage)
        {
            string unsubscribeURL = amazonSnsMessage.UnsubscribeURL;

            Uri confirmUri;
            bool uriIsValid = Uri.TryCreate(unsubscribeURL, UriKind.Absolute, out confirmUri);
            if (!uriIsValid)
            {
                if (_logger != null)
                    _logger.Error("Неверный адрес отказа от подписки Amazon Sns {0}", unsubscribeURL);
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
                if (_logger != null)
                    _logger.Exception(ex, "Ошибка при получении ответа на отказ от подписки Amazon Sns.");
                return false;
            }

            if (_logger != null)
                _logger.Debug("Получен ответ на отказ от подписки Amazon Sns: {0}", response);

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
                if (_logger != null)
                    _logger.Exception(ex, "Ошибка при проверке ответа на отказ от подписки Amazon Sns: {0}", response);
            }

            return result;
        }
    }
}
