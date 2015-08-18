using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL.Entities;
using SignaloBot.DAL.QueriesNDR;
using SignaloBot.DAL;
using SignaloBot.DAL.Enums;
using SignaloBot.DAL.Queries.NDR;
using SignaloBot.DAL.Entities.Core;

namespace SignaloBot.NDR.Model
{
    public class NDRHandler
    {
        //поля
        protected int _ndrCountToBlock = 3;
        protected ICommonLogger _logger;
        protected INDRQueries _ndrQueries;
        protected INDRParser _ndrParser;
        protected int _deliveryType;


        //свойства
        /// <summary>
        /// Максимальное количество некритичных ошибок доставки до блокировки адреса почты. Значение по умолчанию 3.
        /// </summary>
        public int NDRCountToBlock
        {
            get { return _ndrCountToBlock; }
            set { _ndrCountToBlock = value; }
        }

        internal ICommonLogger Logger
        {
            get { return _logger; }
        }


        //инициализация
        public NDRHandler(int deliveryType, INDRParser ndrParser, INDRQueries ndrQueries, ICommonLogger logger)
        {
            _deliveryType = deliveryType;
            _ndrParser = ndrParser;
            _ndrQueries = ndrQueries;
            _logger = logger;
        }


        //методы
        /// <summary>
        /// Обработать входящий поток amazon SNS сообщения с оповещением о недоставленном письме.
        /// </summary>
        /// <param name="messageStream"></param>
        public void Handle(Stream requestStream)
        {
            string requestMessage = ReadRequestStream(requestStream);

            Handle(requestMessage);
        }

        /// <summary>
        /// Обработать строку amazon SNS сообщения с оповещением о недоставленном письме.
        /// </summary>
        /// <param name="messageString"></param>
        public void Handle(string requestMessage)
        {
            List<BouncedMessage> bouncedMessages = _ndrParser.ParseBounceInfo(requestMessage);

            //обновить UserSettings
            List<string> addressesBounced = bouncedMessages.Select(p => p.ReceiverAddress)
                .Distinct().ToList();
            List<UserDeliveryTypeSettings> userSettings =
                _ndrQueries.NDRSettings_Select(_deliveryType, addressesBounced);
            UpdateUserSettings(bouncedMessages, userSettings);

            //добавить BouncedMessages
            SetReceiverUserIds(bouncedMessages, userSettings);
            _ndrQueries.BouncedMessage_Insert(bouncedMessages);
        }
        
        /// <summary>
        /// Прочитать входящий поток, но не производить обработку.
        /// </summary>
        /// <param name="requestStream"></param>
        /// <returns></returns>
        public string ReadRequestStream(Stream requestStream)
        {
            string requestMessage = null;

            try
            {
                using (StreamReader reader = new StreamReader(requestStream))
                {
                    requestMessage = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.Exception(ex, "Ошибка при чтении потока данных NDR сообщения.");
                }
            }

            return requestMessage;
        }

        
        private void UpdateUserSettings(List<BouncedMessage> bouncedMessages, List<UserDeliveryTypeSettings> userSettings)
        {
            var updatedUserSettings = new List<UserDeliveryTypeSettings>();


            //проверить счётчик недоставленных сообщений по каждому адресу
            foreach (BouncedMessage bounce in bouncedMessages)
            {
                if (string.IsNullOrEmpty(bounce.ReceiverAddress))
                    continue;

                UserDeliveryTypeSettings userDeliveryTypeSettings = userSettings
                    .FirstOrDefault(p => p.Address == bounce.ReceiverAddress);

                //адрес, который не привязан ни к одному пользователю
                if (userDeliveryTypeSettings == null)
                    continue;

                userDeliveryTypeSettings.NDRCount++;

                //изменить статус блокировки
                bool newIsBlockedOfNDR = userDeliveryTypeSettings.IsBlockedOfNDR
                    || userDeliveryTypeSettings.NDRCount >= NDRCountToBlock
                    || bounce.BounceType == BounceType.HardBounce;

                userDeliveryTypeSettings.IsBlockedOfNDR = newIsBlockedOfNDR;
                updatedUserSettings.Add(userDeliveryTypeSettings);
            }


            //обновить настройки UserSettings
            if (updatedUserSettings.Count > 0)
            {
                _ndrQueries.NDRSettings_Update(updatedUserSettings);
            }
        }

        private void SetReceiverUserIds(List<BouncedMessage> bouncedMessages, List<UserDeliveryTypeSettings> userSettings)
        {
            //проверить счётчик недоставленных сообщений по каждому адресу
            foreach (BouncedMessage bounce in bouncedMessages)
            {
                if (string.IsNullOrEmpty(bounce.ReceiverAddress))
                    continue;

                UserDeliveryTypeSettings userDeliveryTypeSettings = userSettings
                    .FirstOrDefault(p => p.Address == bounce.ReceiverAddress);

                //адрес, который не привязаные ни к одному пользователю
                if (userDeliveryTypeSettings == null)
                    continue;

                //установить UserID для недоставленного сообщения
                bounce.ReceiverUserID = userDeliveryTypeSettings.UserID;
            }
        }
        
    }
}
