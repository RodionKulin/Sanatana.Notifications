using SignaloBot.Amazon;
using SignaloBot.Amazon;
using Amazon.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.Tests
{
    [TestClass()]    
    public class AmazonCredentialsTests
    {
        [TestMethod()]
        public void AmazonCredentials_InitTest()
        {
            string regionEndpoint = "eu-west-1";
            AmazonCredentials target = new AmazonCredentials(regionEndpoint);   
        }
        
        [TestMethod()]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void AmazonCredentials_InitErrorTest()
        {
            string regionEndpoint = "Westerosis";
            AmazonCredentials target = new AmazonCredentials(regionEndpoint);
        }
    }
}
