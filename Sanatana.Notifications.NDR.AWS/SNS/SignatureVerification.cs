using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR.AWS.SNS
{
    public class SignatureVerification
    {
        //fields
        protected ILogger _logger;


        //init
        public SignatureVerification(ILogger logger)
        {
            _logger = logger;
        }



        //проверить
        public bool VerifySignature(AmazonSnsMessage amazonSnsMessage)
        {
            Uri signingCertUri = null;

            // verified message properties
            if (amazonSnsMessage.SignatureVersion != AmazonConstansts.SNS_SUPPORTED_SIGNATURE_VERSION)
            {
                _logger.LogError($"Unknown SNS message type {amazonSnsMessage.SignatureVersion}, when {AmazonConstansts.SNS_SUPPORTED_SIGNATURE_VERSION} wat expected.");
                return false;
            }

            bool validUri = Uri.TryCreate(amazonSnsMessage.SigningCertURL, UriKind.Absolute, out signingCertUri);
            if (!validUri)
            {
                _logger.LogError($"Unknown SNS certificate address {amazonSnsMessage.SigningCertURL}.");
                return false;
            }

            if (signingCertUri.Host.EndsWith(AmazonConstansts.SNS_SIGNING_CERTIFICATE_URL_END) == false)
            {
                _logger.LogError($"Invalid SNS certificate address {amazonSnsMessage.SigningCertURL}, when expecting {AmazonConstansts.SNS_SIGNING_CERTIFICATE_URL_END}");
                return false;
            }

            if (string.IsNullOrEmpty(amazonSnsMessage.Signature))
            {
                _logger.LogError("SNS message signature is not present.");
                return false;
            }


            // construct message to hash and compare
            string generatedMessage = amazonSnsMessage.GenerateContentString();

            // download certificate
            byte[] pemFileBytes = DownloadCertificate(signingCertUri);

            // verify
            bool verified = CompareSignature(generatedMessage, amazonSnsMessage.Signature, pemFileBytes);
            return verified;
        }

        private bool CompareSignature(string generatedMessage, string signatureFromAmazon, byte[] pemFileBytes)
        {
            bool verified = false;

            byte[] signatureBytes;
            signatureBytes = Convert.FromBase64String(signatureFromAmazon);

            X509Certificate2 x509Certificate2 = new X509Certificate2(pemFileBytes);
            RSACryptoServiceProvider rsaCryptoServiceProvider;
            rsaCryptoServiceProvider = (RSACryptoServiceProvider)x509Certificate2.PublicKey.Key;

            SHA1Managed sha1Managed = new SHA1Managed();
            byte[] hashBytes = sha1Managed.ComputeHash(Encoding.UTF8.GetBytes(generatedMessage));
            verified = rsaCryptoServiceProvider.VerifyHash(hashBytes, CryptoConfig.MapNameToOID("SHA1"), signatureBytes);

            return verified;
        }

        private byte[] DownloadCertificate(Uri signingCertUri)
        {
            byte[] pemFileBytes = null;

            try
            {
                using (HttpClient webClient = new HttpClient())
                {
                    pemFileBytes = webClient.GetByteArrayAsync(signingCertUri).Result;
                }
            }
            catch (Exception webException)
            {
                _logger.LogError(webException, null);
            }

            return pemFileBytes;
        }

    }
}
