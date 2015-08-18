using SignaloBot.DAL.Entities;
using SignaloBot.DAL.Entities.Core;
using SignaloBot.Sender.Senders;
using SignaloBot.Sender.Queue;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders.Email
{
    public class SmtpEmailSender : ISender<Signal>
    {
        //поля
        private ICommonLogger _logger;
        private SmtpSettings _smtpSettings;


        //свойства
        /// <summary>
        /// Адрес, который будет использован для проверки доступности метода отправки сообщений. На него будет отправлено тестовое сообщение.
        /// </summary>
        public string AvailabilityCheckEmailAddress { get; set; }


        //инициализация
        public SmtpEmailSender(ICommonLogger logger, SmtpSettings smtpSettings)
        {
            _logger = logger;
            _smtpSettings = smtpSettings;
        }



        //методы
        public virtual SendResult Send(Signal message)
        {
            bool result = false;

            MailAddress mailSender = new MailAddress(message.SenderAddress, message.SenderDisplayName);
            MailAddress mailReceiver = new MailAddress(message.ReceiverAddress, message.ReceiverDisplayName);
            MailMessage mailMessage = new MailMessage(mailSender, mailReceiver);
            mailMessage.Subject = message.MessageSubject;
            mailMessage.Body = message.MessageBody;
            mailMessage.IsBodyHtml = message.IsBodyHtml;

            using (SmtpClient client = ConstructSmtpClient())
            {
                try
                {
                    client.Send(mailMessage);
                    result = true;
                }
                catch (SmtpFailedRecipientsException ex)
                {
                    if (_logger != null)
                    {
                        _logger.Exception(ex, "Smtp ошибка доставки почты по адресу {0}", message.ReceiverAddress);
                    }
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logger.Exception(ex, "Ошибка доставки почты по адресу {0}", message.ReceiverAddress);
                    }
                }
            }

            return result
                ? SendResult.Success
                : SendResult.Fail;
        }

        protected virtual SmtpClient ConstructSmtpClient()
        {
            SmtpClient client = new SmtpClient();
            client.Host = _smtpSettings.Server;
            client.Port = _smtpSettings.Port;
            client.EnableSsl = _smtpSettings.EnableSsl;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Timeout = 15000;
            client.UseDefaultCredentials = _smtpSettings.Credentials == null;
            client.Credentials = _smtpSettings.Credentials;

            return client;
        }

        public SenderAvailability CheckAvailability()
        {
            if(AvailabilityCheckEmailAddress == null)
            {
                return SenderAvailability.NotChecked;
            }

            SendResult result = Send(new Signal()
            {
                ReceiverAddress = AvailabilityCheckEmailAddress,
                SenderAddress = AvailabilityCheckEmailAddress,
                MessageBody = "test message",
                MessageSubject = "test message",
                IsBodyHtml = false
            });

            return result == SendResult.Success
                ? SenderAvailability.Available
                : SenderAvailability.NotAvailable;
        }
        

        //IDisposable
        public virtual void Dispose()
        {
        }

    }
}
