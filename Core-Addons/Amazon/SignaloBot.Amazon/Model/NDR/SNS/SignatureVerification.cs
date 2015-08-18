using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Amazon.NDR.SNS
{
    internal class SignatureVerification
    {
        ICommonLogger _logger;


        //инициализация
        public SignatureVerification(ICommonLogger logger)
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
                if (_logger != null)
                {
                    _logger.Error("Неизвестный тип подписи Amazon Sns {0}, когда ожидалось {1}."
                        , amazonSnsMessage.SignatureVersion, AmazonConstansts.SNS_SUPPORTED_SIGNATURE_VERSION);
                }
                return false;
            }

            bool validUri = Uri.TryCreate(amazonSnsMessage.SigningCertURL, UriKind.Absolute, out signingCertUri);
            if (!validUri)
            {
                if (_logger != null)
                {
                    _logger.Error("Неверный адрес сертификата Amazon Sns {0}.", amazonSnsMessage.SigningCertURL);
                }
                return false;
            }

            if (signingCertUri.Host.EndsWith(AmazonConstansts.SNS_SIGNING_CERTIFICATE_URL_END) == false)
            {
                if (_logger != null)
                {
                    _logger.Error("Неверный адрес сертификата Amazon Sns {0}, когда ожидалось."
                        , amazonSnsMessage.SigningCertURL, AmazonConstansts.SNS_SIGNING_CERTIFICATE_URL_END);
                }
                return false;
            }

            if (string.IsNullOrEmpty(amazonSnsMessage.Signature))
            {
                if (_logger != null)
                {
                    _logger.Error("Отсутствует подпись сообщения Amazon Sns.");
                }
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
                using (WebClient webClient = new WebClient())
                {
                    pemFileBytes = webClient.DownloadData(signingCertUri);
                }
            }
            catch (Exception webException)
            {
                if (_logger != null)
                    _logger.Exception(webException, "Ошибка при загрузке сертификата Amazon SNS.");
            }

            return pemFileBytes;
        }

    }
}
