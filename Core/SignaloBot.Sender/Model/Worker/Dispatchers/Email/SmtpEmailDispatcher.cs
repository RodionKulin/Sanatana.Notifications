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
using SignaloBot.DAL;
using SignaloBot.Sender.Resources;
using SignaloBot.Sender.Processors;

namespace SignaloBot.Sender.Senders.Email
{
    public class SmtpEmailDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //поля
        protected ICommonLogger _logger;
        protected SmtpSettings _smtpSettings;


        //свойства
        /// <summary>
        /// Адрес, который будет использован для проверки доступности отправки. На него будет отправлено тестовое сообщение.
        /// </summary>
        public virtual string AvailabilityCheckEmailAddress { get; set; }


        //инициализация
        public SmtpEmailDispatcher(ICommonLogger logger, SmtpSettings smtpSettings)
        {
            _logger = logger;
            _smtpSettings = smtpSettings;
        }



        //методы
        public virtual ProcessingResult Send(SignalDispatchBase<TKey> item)
        {
            if(!(item is SubjectDispatch<TKey>))
            {
                if (_logger != null)
                {
                    _logger.Error(InternalMessages.SmtpEmailSender_WrongType, item.GetType(), typeof(SubjectDispatch<TKey>));
                }

                return ProcessingResult.Fail;
            }

            SubjectDispatch<TKey> signal = item as SubjectDispatch<TKey>;
            bool result = false;

            try
            {
                MailAddress mailSender = new MailAddress(signal.SenderAddress, signal.SenderDisplayName);
                MailAddress mailReceiver = new MailAddress(signal.ReceiverAddress, signal.ReceiverDisplayName);
                MailMessage mailMessage = new MailMessage(mailSender, mailReceiver);

                mailMessage.Subject = signal.MessageSubject;
                mailMessage.Body = signal.MessageBody;
                mailMessage.IsBodyHtml = signal.IsBodyHtml;

                using (SmtpClient client = ConstructSmtpClient())
                {
                    client.Send(mailMessage);
                    result = true;
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                if (_logger != null)
                {
                    _logger.Exception(ex, InternalMessages.SmtpEmailSender_SmtpFail, signal.ReceiverAddress);
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.Exception(ex, InternalMessages.SmtpEmailSender_Fail, signal.ReceiverAddress);
                }
            }

            return result
                ? ProcessingResult.Success
                : ProcessingResult.Fail;
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

        public virtual DispatcherAvailability CheckAvailability()
        {
            if (AvailabilityCheckEmailAddress == null)
            {
                return DispatcherAvailability.NotChecked;
            }

            ProcessingResult result = Send(new SubjectDispatch<TKey>()
            {
                ReceiverAddress = AvailabilityCheckEmailAddress,
                SenderAddress = AvailabilityCheckEmailAddress,
                MessageBody = "test item",
                MessageSubject = "test item",
                IsBodyHtml = false
            });

            return result == ProcessingResult.Success
                ? DispatcherAvailability.Available
                : DispatcherAvailability.NotAvailable;
        }



        //IDisposable
        public virtual void Dispose()
        {
        }

    }
}
