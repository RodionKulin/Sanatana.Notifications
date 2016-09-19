using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Common.Utility;
using Amazon.Runtime;
using SignaloBot;
using SignaloBot.Sender;
using SignaloBot.TestParameters.Model;
using SignaloBot.TestParameters.Model.Stubs;
using SignaloBot.Amazon;
using SignaloBot.Amazon.Sender;
using SignaloBot.Sender.Senders;
using SignaloBot.DAL;
using SignaloBot.Sender.Processors;

namespace SignaloBot.Amazon.Tests
{
    
    [TestClass()]
    public class AmazonEmailSenderTest
    {
        [TestMethod()]
        [Ignore]
        public void SendAmazonEmailTest()
        {
            //setup
            ICommonLogger logger = new CommonLogStub();
            AmazonCredentials amazonCredentials = new AmazonCredentials(AmazonTestParameters.AWSRegion
                , AmazonTestParameters.AWSAccessKey, AmazonTestParameters.AWSSecretKey);
            AmazonEmailSender<Guid> target = new AmazonEmailSender<Guid>(amazonCredentials, logger);
            var email = new SubjectDispatch<Guid>()
            {
                SenderAddress = AmazonTestParameters.SenderEmail,
                SenderDisplayName = "sender display name here",
                ReceiverAddress = AmazonTestParameters.ReceiverEmail,
                ReceiverDisplayName = "receiver display name here",

                MessageSubject = "mail subject",
                MessageBody = "mail body",
                IsBodyHtml = false,

                IsDelayed = false
            };

            //test     
            ProcessingResult actual = target.Send(email);
            Assert.AreEqual(ProcessingResult.Success, actual);
        }

    }
}
