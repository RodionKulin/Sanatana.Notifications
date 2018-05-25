using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.Notifications.NDR.AWS.SES
{
    public class AmazonSesManager
    {
        //fields
        protected ILogger _logger;

                

        //init
        public AmazonSesManager(ILogger logger)
        {
            _logger = logger;
        }



        //methods
        public bool ParseRequest(string json, out AmazonSesNotification notification)
        {
            notification = null;
            
            // parse SES message
            AmazonSesNotification amazonSesNotification;
            bool created = AmazonSesNotification.TryCreate(json, out amazonSesNotification);

            if (!created)
            {
                _logger.LogError($"SES json message was not successfuly parsed: {json}");
                return false;
            }

            notification = amazonSesNotification;
            return true;
        }
    }
}