using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.Amazon.NDR.SNS;
using SignaloBot.Amazon.NDR.SES;
using System.Xml.Linq;
using SignaloBot.Amazon.Enums;
using SignaloBot.Amazon;
using SignaloBot.DAL.Entities;
using SignaloBot.NDR.Model;

namespace SignaloBot.Amazon.NDR
{
    public class AmazonNDRParser : INDRParser
    {
        //поля
        AmazonSnsManager _amazonSnsManager;
        AmazonSesManager _amazonSesManager;
        ICommonLogger _logger;


        //свойства
        /// <summary>
        /// Адрес отправителя письма. Если указан, то обрабатываются только письма с указанным отправителем.
        /// </summary>
        public string SourceAddressToVerify { get; set; }


        //инициализация
        public AmazonNDRParser(ICommonLogger logger)
        {
            _logger = logger;
            _amazonSnsManager = new AmazonSnsManager(_logger);
            _amazonSesManager = new AmazonSesManager(_logger);
        }



        //методы
        /// <summary>
        /// Обработать строку amazon SNS сообщения с оповещением о недоставленном письме.
        /// </summary>
        /// <param name="messageString"></param>
        public List<BouncedMessage> ParseBounceInfo(string requestMessage)
        {
            //string => sns message
            AmazonSnsMessage message;
            bool isValid = _amazonSnsManager.ParseRequest(requestMessage, out message);
            if (isValid == false)
                return new List<BouncedMessage>();
           
            //sns message => ses message
            AmazonSesNotification sesNotification = ExtractSesNotification(message);
            if (sesNotification == null)
                return new List<BouncedMessage>();

            //ses message => List<BouncedMessage>
            return CreateBounceList(sesNotification);
        }

        private AmazonSesNotification ExtractSesNotification(AmazonSnsMessage message)
        {
            AmazonSesNotification sesNotification;
            bool isValid = _amazonSesManager.ParseRequest(message.Message, out sesNotification);
            if (isValid == false)            
                return null;

            //проверить отправителя
            bool sourceVerificationEnabled = !string.IsNullOrEmpty(SourceAddressToVerify);

            if (sourceVerificationEnabled
                && sesNotification.Mail.Source != SourceAddressToVerify)
            {
                if (_logger != null)
                {
                    _logger.Error("Получено уведомление о недоставленном письме неожиданного отправителя {0}, когда ожидалось {1}."
                        , sesNotification.Mail.Source, SourceAddressToVerify);
                }
                return null;
            }

            return sesNotification;
        }               

        private List<BouncedMessage> CreateBounceList(AmazonSesNotification sesNotification)
        {
            List<BouncedMessage> bouncedMessages = new List<BouncedMessage>();
           
            if (sesNotification.AmazonSesMessageType == AmazonSesMessageType.Unknown)
            {
                if (_logger != null)               
                    _logger.Error("Получен неизвестный тип сообщения Amazon SES.");
                return bouncedMessages;
            }

            //разобрать NDR
            else if (sesNotification.AmazonSesMessageType == AmazonSesMessageType.Bounce)
            {
                foreach (AmazonSesBouncedRecipient recipient in sesNotification.Bounce.BouncedRecipients)
                {
                    DAL.Enums.BounceType bounceType = sesNotification.Bounce.AmazonBounceType == AmazonBounceType.Permanent
                            ? DAL.Enums.BounceType.HardBounce
                            : DAL.Enums.BounceType.SoftBounce;

                    string detailsXml = XmlBounceDetails.DetailsToXml(sesNotification.AmazonSesMessageType
                        , sesNotification.Bounce.AmazonBounceType, sesNotification.Bounce.AmazonBounceSubType);
                                          
                    BouncedMessage bouncedMessage = CreateBouncedMessage(bounceType
                          , sesNotification.Mail, recipient.EmailAddress, detailsXml);

                    bouncedMessages.Add(bouncedMessage);
                }
            }

            //разобрать жалобу
            else if (sesNotification.AmazonSesMessageType == AmazonSesMessageType.Complaint)
            {
                foreach (AmazonSesComplaintRecipient recipient in sesNotification.Complaint.ComplainedRecipients)
                {
                    string detailsXml = XmlBounceDetails.DetailsToXml(sesNotification.AmazonSesMessageType
                        , complaintFeedbackType: sesNotification.Complaint.AmazonComplaintFeedbackType);

                    BouncedMessage bouncedMessage = CreateBouncedMessage(DAL.Enums.BounceType.HardBounce
                        , sesNotification.Mail, recipient.EmailAddress, detailsXml);

                    bouncedMessages.Add(bouncedMessage);
                }
            }

            return bouncedMessages;
        }

        private BouncedMessage CreateBouncedMessage(DAL.Enums.BounceType type, AmazonSesMail mail
            , string recipientEmail, string detailsXml)
        {
            BouncedMessage bouncedMessage = new BouncedMessage()
            {
                ReceiverAddress = recipientEmail,

                BounceType = type,
                BounceReceiveDateUtc = DateTime.UtcNow,
                BounceDetailsXML = detailsXml,

                MessageBody = null,
                MessageSubject = null,
                SendDateUtc = null 

                //DeliveryType, ReceiverUserID, BouncedMessageID
                //устанавливаются через NDRHandler
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
