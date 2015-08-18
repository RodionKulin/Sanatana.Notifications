using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml.Linq;
using SignaloBot.DAL;
using SignaloBot.Amazon.Enums;

namespace SignaloBot.Amazon.NDR.SNS
{
    internal class AmazonSnsManager
    {

        bool _confirmSubsription = true;
        ICommonLogger _logger;
        SignatureVerification _signatureVerification;
        Subscription _subscription;


        // свойства
        /// <summary>
        /// Автоматически подтверждать подписку на получение SNS сообщение по этому адресу.
        /// </summary>
        public bool ConfirmSubsription
        {
            get { return _confirmSubsription; }
            set { _confirmSubsription = value; }
        }


        // инициализация
        public AmazonSnsManager(ICommonLogger logger)
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
                if(_logger != null)
                    _logger.Exception(ex, "Ошибка при получении потока данных Amazon SNS.");
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

            // разбор json сообщения
            AmazonSnsMessage amazonSnsMessage;
            bool created = AmazonSnsMessage.TryCreate(request, out amazonSnsMessage);
            if (!created)
            {
                if (_logger != null)
                    _logger.Error("Ошибка при разборе json сообщения Amazon Sns: {0}", request);
                return false;
            }

            // проверить подпись сообщения
            bool verified = _signatureVerification.VerifySignature(amazonSnsMessage);
            if (!verified)
            {
                if (_logger != null)
                    _logger.Error("Не удалось подтвердить подпись сообщения Amazon Sns: {0}", request);
                return false;
            }

            
            // обработать сообщение в зависимости от типа
            if (amazonSnsMessage.AmazonSnsMessageType == AmazonSnsMessageType.Notification)
            {
                message = amazonSnsMessage;
                isValid = message.Message != null;
            }

            // подписаться
            else if (amazonSnsMessage.AmazonSnsMessageType == AmazonSnsMessageType.SubscriptionConfirmation
                && ConfirmSubsription)
            {
                isValid = _subscription.ConfirmSubscription(amazonSnsMessage);
            }

            // неизвестный тип
            else
            {
                if (_logger != null)
                    _logger.Error("Получен неизвестный тип сообщения Amazon Sns: {0}", request);
                isValid = false;
            }

            return isValid;
        }
    }
}