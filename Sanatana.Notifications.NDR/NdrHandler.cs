using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Enums;
using Microsoft.Extensions.Logging;

namespace Sanatana.Notifications.NDR
{
    public class NdrHandler<TKey> : INdrHandler
        where TKey : struct
    {
        //fields
        protected ISignalBounceQueries<TKey> _bouncedQueries;
        protected ISubscriberDeliveryTypeSettingsQueries<TKey> _subscriberQueries;
        protected INdrParser<TKey> _ndrParser;
        protected ILogger _logger;


        //properties
        /// <summary>
        /// Maximum number of warnings before blocking delivery address to be used again.
        /// </summary>
        public int NDRCountToBlock { get; set; } = 3;
        public bool LogIncomingMessages { get; set; }
        public int DeliveryType { get; set; }



        //init
        public NdrHandler()
        {
        }
        public NdrHandler(int deliveryType, ILogger logger, INdrParser<TKey> ndrParser
            , ISignalBounceQueries<TKey> bouncedQueries, ISubscriberDeliveryTypeSettingsQueries<TKey> subscriberQueries)
        {
            DeliveryType = deliveryType;
            _logger = logger;
            _ndrParser = ndrParser;
            _bouncedQueries = bouncedQueries;
            _subscriberQueries = subscriberQueries;
        }


        //methods
        /// <summary>
        /// Handle incoming message stream.
        /// </summary>
        /// <param name="requestStream"></param>
        /// <returns></returns>
        public Task Handle(Stream requestStream)
        {
            string requestMessage = ReadRequestStream(requestStream);

            return Handle(requestMessage);
        }

        /// <summary>
        /// Handle incoming message string.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public async Task Handle(string requestMessage)
        {
            if (LogIncomingMessages)
            {
                _logger.LogInformation(@"NDR Message received:
{0}", requestMessage);
            }

            List<SignalBounce<TKey>> bouncedMessages = _ndrParser.ParseBounceInfo(requestMessage);
            List<string> addressesBounced = bouncedMessages.Select(p => p.ReceiverAddress)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct().ToList();

            //update SubscriberSettings
            if (addressesBounced.Count > 0)
            {
                List<SubscriberDeliveryTypeSettings<TKey>> subscriberSettings =
                    await _subscriberQueries.Select(DeliveryType, addressesBounced);

                await UpdateSubscriberSettings(bouncedMessages, subscriberSettings).ConfigureAwait(false);
               
                SetReceiverSubscriberIds(bouncedMessages, subscriberSettings);
            }

            //add BouncedMessages
            if (bouncedMessages.Count > 0)
            {
                await _bouncedQueries.Insert(bouncedMessages);
            }
        }
        
        /// <summary>
        /// Read incoming stream to string.
        /// </summary>
        /// <param name="requestStream"></param>
        /// <returns></returns>
        public string ReadRequestStream(Stream requestStream)
        {
            string requestMessage = null;

            using (StreamReader reader = new StreamReader(requestStream))
            {
                requestMessage = reader.ReadToEnd();
            }

            return requestMessage;
        }
                
        private Task UpdateSubscriberSettings(List<SignalBounce<TKey>> bouncedMessages, List<SubscriberDeliveryTypeSettings<TKey>> subscriberSettings)
        {
            var updatedSubscriberSettings = new List<SubscriberDeliveryTypeSettings<TKey>>();


            //check number of not delivered messages for each address
            foreach (SignalBounce<TKey> bounce in bouncedMessages)
            {
                if (string.IsNullOrEmpty(bounce.ReceiverAddress))
                    continue;

                SubscriberDeliveryTypeSettings<TKey> subscriberDeliveryTypeSettings = subscriberSettings
                    .FirstOrDefault(p => p.Address == bounce.ReceiverAddress);

                //address that is not wired to any subscriber
                if (subscriberDeliveryTypeSettings == null)
                    continue;

                subscriberDeliveryTypeSettings.NDRCount++;

                //change block state
                bool newIsBlockedOfNDR = subscriberDeliveryTypeSettings.IsNDRBlocked
                    || subscriberDeliveryTypeSettings.NDRCount >= NDRCountToBlock
                    || bounce.BounceType == BounceType.HardBounce;

                subscriberDeliveryTypeSettings.IsNDRBlocked = newIsBlockedOfNDR;
                updatedSubscriberSettings.Add(subscriberDeliveryTypeSettings);
            }


            //update SubscriberSettings
            if (updatedSubscriberSettings.Count > 0)
            {
                return _subscriberQueries.UpdateNDRSettings(updatedSubscriberSettings);
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        private void SetReceiverSubscriberIds(List<SignalBounce<TKey>> bouncedMessages, List<SubscriberDeliveryTypeSettings<TKey>> subscriberSettings)
        {
            foreach (SignalBounce<TKey> bounce in bouncedMessages)
            {
                if (string.IsNullOrEmpty(bounce.ReceiverAddress))
                    continue;

                SubscriberDeliveryTypeSettings<TKey> subscriberDeliveryTypeSettings = subscriberSettings
                    .FirstOrDefault(p => p.Address == bounce.ReceiverAddress);

                if (subscriberDeliveryTypeSettings == null)
                    continue;

                bounce.ReceiverSubscriberId = subscriberDeliveryTypeSettings.SubscriberId;
            }
        }
        
    }
}
