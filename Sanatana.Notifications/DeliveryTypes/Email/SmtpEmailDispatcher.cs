using Sanatana.Notifications.DispatchHandling;
using Sanatana.Notifications.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.DispatchHandling.Channels;
using Sanatana.Notifications.DAL.Entities;
using Microsoft.Extensions.Logging;
using Sanatana.Notifications.Resources;

namespace Sanatana.Notifications.DeliveryTypes.Email
{
    public class SmtpEmailDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //fields
        protected SmtpSettings _smtpSettings;


        //properties
        /// <summary>
        /// Address that will be used to check email channel availability. Sample email will be forwarded on check.
        /// </summary>
        public virtual string AvailabilityCheckEmailAddress { get; set; }


        //init
        public SmtpEmailDispatcher(SmtpSettings smtpSettings)
        {
            _smtpSettings = smtpSettings;
        }



        //methods
        public virtual async Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            EmailDispatch<TKey> emailDispatch = (EmailDispatch<TKey>)item;

            MailMessage mailMessage = BuildMessage(emailDispatch);
            using (SmtpClient client = BuildSmtpClient())
            {
                await client.SendMailAsync(mailMessage);
            }

            return ProcessingResult.Success;
        }

        protected virtual MailMessage BuildMessage(EmailDispatch<TKey> emailDispatch)
        {
            MailAddress mailSender = new MailAddress(_smtpSettings.SenderAddress, _smtpSettings.SenderDisplayName);
            MailAddress mailReceiver = new MailAddress(emailDispatch.ReceiverAddress, emailDispatch.ReceiverDisplayName);
            MailMessage mailMessage = new MailMessage(mailSender, mailReceiver);

            mailMessage.Subject = emailDispatch.MessageSubject;
            mailMessage.Body = emailDispatch.MessageBody;
            mailMessage.IsBodyHtml = emailDispatch.IsBodyHtml;

            if (emailDispatch.CCAddresses != null)
            {
                foreach (string cc in emailDispatch.CCAddresses)
                {
                    mailMessage.CC.Add(cc.Trim());
                }
            }
            if (emailDispatch.BCCAddresses != null)
            {
                foreach (string bcc in emailDispatch.BCCAddresses)
                {
                    mailMessage.Bcc.Add(bcc.Trim());
                }
            }
            if (emailDispatch.ReplyToAddresses != null)
            {
                foreach (ReplyToAddress replyTo in emailDispatch.ReplyToAddresses)
                {
                    mailMessage.ReplyToList.Add(new MailAddress(replyTo.Address, replyTo.DisplayName));
                }
            }

            return mailMessage;
        }

        protected virtual SmtpClient BuildSmtpClient()
        {
            SmtpClient client = new SmtpClient();
            client.Host = _smtpSettings.Server;
            client.Port = _smtpSettings.Port;
            client.EnableSsl = _smtpSettings.EnableSsl;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Timeout = 15000;

            //UseDefaultCredentials = false must go before Credentials = ..., cause it will set Credentials to null.
            client.UseDefaultCredentials = _smtpSettings.Credentials == null;   
            client.Credentials = _smtpSettings.Credentials;

            return client;
        }

        public virtual async Task<DispatcherAvailability> CheckAvailability()
        {
            if (AvailabilityCheckEmailAddress == null)
            {
                return DispatcherAvailability.NotChecked;
            }

            ProcessingResult result = await Send(new EmailDispatch<TKey>()
            {
                ReceiverAddress = AvailabilityCheckEmailAddress,
                MessageBody = "test item",
                MessageSubject = "test item",
                IsBodyHtml = false
            });

            return result == ProcessingResult.Success
                ? DispatcherAvailability.Available
                : DispatcherAvailability.NotAvailable;
        }

        public virtual void Dispose()
        {
        }
    }
}
