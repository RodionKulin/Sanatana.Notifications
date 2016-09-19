using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignaloBot.DAL;

namespace SignaloBot.NDR.Model
{
    public class NDRHandler<TKey>
        where TKey : struct
    {
        //поля
        protected int _ndrCountToBlock = 3;
        protected ICommonLogger _logger;
        protected ISignalBounceQueries<TKey> _bouncedQueries;
        protected IUserDeliveryTypeSettingsQueries<TKey> _userQueries;
        protected INDRParser<TKey> _ndrParser;
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

        public int DeliveryType
        {
            get { return _deliveryType; }
            set { _deliveryType = value; }
        }



        //инициализация
        public NDRHandler()
        {
        }
        public NDRHandler(int deliveryType, ICommonLogger logger, INDRParser<TKey> ndrParser
            , ISignalBounceQueries<TKey> bouncedQueries, IUserDeliveryTypeSettingsQueries<TKey> userQueries)
        {
            _deliveryType = deliveryType;
            _ndrParser = ndrParser;
            _bouncedQueries = bouncedQueries;
            _userQueries = userQueries;
            _logger = logger;
        }


        //методы
        /// <summary>
        /// Обработать входящий поток amazon SNS сообщения с оповещением о недоставленном письме.
        /// </summary>
        /// <param name="messageStream"></param>
        public Task<bool> Handle(Stream requestStream)
        {
            string requestMessage = ReadRequestStream(requestStream);

            return Handle(requestMessage);
        }

        /// <summary>
        /// Обработать строку amazon SNS сообщения с оповещением о недоставленном письме.
        /// </summary>
        /// <param name="messageString"></param>
        public async Task<bool> Handle(string requestMessage)
        {
            List<SignalBounce<TKey>> bouncedMessages = _ndrParser.ParseBounceInfo(requestMessage);
            List<string> addressesBounced = bouncedMessages.Select(p => p.ReceiverAddress)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct().ToList();

            //обновить UserSettings
            if (addressesBounced.Count > 0)
            {
                QueryResult<List<UserDeliveryTypeSettings<TKey>>> userSettings =
                    await _userQueries.Select(_deliveryType, addressesBounced);
                if (userSettings.HasExceptions)
                {
                    return false;
                }

                bool updated = await UpdateUserSettings(bouncedMessages, userSettings.Result);
                if (!updated)
                {
                    return false;
                }

                SetReceiverUserIds(bouncedMessages, userSettings.Result);
            }

            //добавить BouncedMessages
            if (bouncedMessages.Count > 0)
            {
                return await _bouncedQueries.Insert(bouncedMessages);
            }
            else
            {
                return true;
            }
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

        
        private Task<bool> UpdateUserSettings(List<SignalBounce<TKey>> bouncedMessages, List<UserDeliveryTypeSettings<TKey>> userSettings)
        {
            var updatedUserSettings = new List<UserDeliveryTypeSettings<TKey>>();


            //проверить счётчик недоставленных сообщений по каждому адресу
            foreach (SignalBounce<TKey> bounce in bouncedMessages)
            {
                if (string.IsNullOrEmpty(bounce.ReceiverAddress))
                    continue;

                UserDeliveryTypeSettings<TKey> userDeliveryTypeSettings = userSettings
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
                return _userQueries.UpdateNDRSettings(updatedUserSettings);
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        private void SetReceiverUserIds(List<SignalBounce<TKey>> bouncedMessages, List<UserDeliveryTypeSettings<TKey>> userSettings)
        {
            //проверить счётчик недоставленных сообщений по каждому адресу
            foreach (SignalBounce<TKey> bounce in bouncedMessages)
            {
                if (string.IsNullOrEmpty(bounce.ReceiverAddress))
                    continue;

                UserDeliveryTypeSettings<TKey> userDeliveryTypeSettings = userSettings
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
