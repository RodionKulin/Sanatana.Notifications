using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignaloBot.Amazon.NDR.SES
{
    internal class AmazonSesManager
    {
        //поля
        ICommonLogger _logger;

                

        //инициализация
        public AmazonSesManager(ICommonLogger logger)
        {
            _logger = logger;
        }



        //методы
        public bool ParseRequest(string json, out AmazonSesNotification notification)
        {
            notification = null;
            
            //разбор  сообщения ses
            AmazonSesNotification amazonSesNotification;
            bool created = AmazonSesNotification.TryCreate(json, out amazonSesNotification);

            if (!created)
            {
                if (_logger != null)
                    _logger.Error("Ошибка при разборе json Amazon Ses: {0}", json);
                return false;
            }

            notification = amazonSesNotification;
            return true;
        }
    }
}