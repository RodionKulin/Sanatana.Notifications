using Sanatana.Notifications;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.Dispatching;
using Sanatana.Notifications.Processing;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DeliveryTypes.Email;
using Amazon;
using Sanatana.Notifications.Resources;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.Dispatchers.AWS_SES
{
    public class AmazonEmailDispatcher<TKey> : IDispatcher<TKey>
        where TKey : struct
    {
        //fields
        protected AmazonCredentials _credentials;


        //properties
        public string SenderAddress { get; set; }
        /// <summary>
        /// Address that will be used to check email channel availability. Sample email will be forwarded on check.
        /// </summary>
        public virtual string AvailabilityCheckEmailAddress { get; set; }


        //init
        public AmazonEmailDispatcher(AmazonCredentials credentials)
        {
            _credentials = credentials;
        }


        //methods
        public virtual async Task<ProcessingResult> Send(SignalDispatch<TKey> item)
        {
            EmailDispatch<TKey> signal = (EmailDispatch<TKey>)item;

            using (var client = new AmazonSimpleEmailServiceClient(_credentials.AwsAccessKey,
                _credentials.AwsSecretKey, _credentials.RegionEndpoint))
            {
                SendEmailRequest request = CreateAmazonRequest(signal);
                SendEmailResponse response = null;
                response = await client.SendEmailAsync(request);
            }

            return ProcessingResult.Success;
        }

        protected virtual SendEmailRequest CreateAmazonRequest(EmailDispatch<TKey> message)
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
                Source = SenderAddress,
                Destination = destination,
                Message = mailMessage
            };
            return request;
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


        //IDisposable
        public virtual void Dispose()
        {
        }
    }
}
