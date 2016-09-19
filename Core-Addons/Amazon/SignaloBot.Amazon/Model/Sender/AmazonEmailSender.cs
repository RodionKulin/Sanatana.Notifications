using SignaloBot.Sender;
using SignaloBot.Sender.Senders;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.Sender.Processors;
using SignaloBot.Amazon.Resources;
using SignaloBot.DAL;

namespace SignaloBot.Amazon.Sender
{
    public class AmazonEmailSender<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //свойства
        /// <summary>
        /// Адрес, который будет использован для проверки доступности метода отправки сообщений. На него будет отправлено тестовое сообщение.
        /// </summary>
        public string AvailabilityCheckEmailAddress { get; set; }

        public AmazonCredentials Credentials { get; set; }

        public ICommonLogger Logger { get; set; }


        //инициализация
        public AmazonEmailSender(AmazonCredentials credentials, ICommonLogger logger)
        {
            Credentials = credentials;
            Logger = logger;
        }


        //методы
        public virtual ProcessingResult Send(SignalDispatchBase<TKey> item)
        {
            if (!(item is SubjectDispatch<TKey>))
            {
                if (Logger != null)
                {
                    Logger.Error(SignaloBot.Sender.Resources.InternalMessages.SmtpEmailSender_WrongType
                        , item.GetType(), typeof(SubjectDispatch<TKey>));
                }

                return ProcessingResult.Fail;
            }

            SubjectDispatch<TKey> signal = item as SubjectDispatch<TKey>;
            bool send = false;

            using (var client = new AmazonSimpleEmailServiceClient(Credentials.AwsAccessKey,
                Credentials.AwsSecretKey, Credentials.RegionEndpoint))
            {
                SendEmailRequest request = CreateAmazonRequest(signal);
                SendEmailResponse response = null;
                
                try
                {
                    response = client.SendEmail(request);
                    send = true;
                }
                catch (Exception exception)
                {
                    if (Logger != null)
                    {
                        Logger.Exception(exception, InternalMessages.SendException
                            , signal.ReceiverAddress);
                    }
                }
            }

            return send
                ? ProcessingResult.Success
                : ProcessingResult.Fail;
        }

        protected virtual SendEmailRequest CreateAmazonRequest(SubjectDispatch<TKey> message)
        {
            // Construct an object to contain the recipient address.
            Destination destination = new Destination();
            destination.ToAddresses = new List<string>() { message.ReceiverAddress };

            // Create the subject and body of the message.
            Content contentSubject = new Content(message.MessageSubject);
            Content contentBody = new Content(message.MessageBody);

            Body body = new Body();
            if (message.IsBodyHtml)
                body.Html = contentBody;
            else
                body.Text = contentBody;
            
            // Create a message with the specified subject and body.
            Message mailMessage = new Message(contentSubject, body);

            // Assemble the email.
            SendEmailRequest request = new SendEmailRequest()
            {
                Source = message.SenderAddress,
                Destination = destination,
                Message = mailMessage
            };
            return request;
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
                MessageBody = "test message",
                MessageSubject = "test message",
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
