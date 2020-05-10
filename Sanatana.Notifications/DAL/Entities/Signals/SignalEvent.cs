using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Entities
{
    [Serializable]
    public class SignalEvent<TKey>
        where TKey : struct
    {
        /// <summary>
        /// SignalEventId created by database.
        /// </summary>
        public TKey SignalEventId { get; set; }
        /// <summary>
        /// Data that will be insert into templates. Can be Object or KeyValue data.
        /// </summary>
        public Dictionary<string, string> TemplateDataDict { get; set; }
        /// <summary>
        /// Data that will be deserialized from json and inserted into templates. Can be Object or KeyValue data.
        /// </summary>
        public string TemplateDataObj { get; set; }
        /// <summary>
        /// Date and time of receiving SignalEvent in ISignalProvider.
        /// </summary>
        public DateTime CreateDateUtc { get; set; }
        /// <summary>
        /// Key used to match this SignalEvent with EventSettings.
        /// </summary>
        public int EventKey { get; set; }
        /// <summary>
        /// TopicId used to override default TopicId from EventSettings.
        /// </summary>
        public string TopicId { get; set; }
        /// <summary>
        /// Number of failed attempts to handle this SignalEvent.
        /// </summary>
        public int FailedAttempts { get; set; }
        /// <summary>
        /// Id of EventSettings that was matched by EventKey.
        /// </summary>
        public TKey? EventSettingsId { get; set; }
        /// <summary>
        /// Way of finding subscribers that will receive notifications.
        /// </summary>
        public AddresseeType AddresseeType { get; set; }


        //AddresseeType.SubscriptionParameters - Fetching subscribers and previous progress fetching subscriber ids.
        /// <summary>
        /// Custom data to filter subscribers. To make it usabe need to implement custom ISubscriberQueries. Used with AddresseeType.SubscriptionParameters
        /// </summary>
        public Dictionary<string, string> SubscriberFiltersData { get; set; }
        /// <summary>
        /// Range of subscriber's Ids that will be queried. Used with AddresseeType.SubscriptionParameters
        /// </summary>
        public TKey? SubscriberIdRangeFrom { get; set; }
        /// <summary>
        /// Range of subscriber's Ids that will be queried. Used with AddresseeType.SubscriptionParameters
        /// </summary>
        public TKey? SubscriberIdRangeTo { get; set; }
        /// <summary>
        /// SubscriberIdRangeFrom subscriber may have multiple deliveryType. 
        /// Store already handled deliveryType to exlucle in future requests.
        /// Used with AddresseeType.SubscriptionParameters
        /// </summary>
        public List<int> SubscriberIdFromDeliveryTypesHandled { get; set; }


        //AddresseeType.SubscriberIds
        /// <summary>
        /// Subscribers that will receive notifications. 
        /// Their notification settings will still be checked if they are enabled acording to subscribtion query in EventSettings.
        /// Used with AddresseeType.SubscriberIds
        /// </summary>
        public List<TKey> PredefinedSubscriberIds { get; set; }


        //AddresseeType.DirectAddresses
        /// <summary>
        /// Direct addresses that will receive notifications. Used with AddresseeType.DirectAddresses
        /// </summary>
        public List<DeliveryAddress> PredefinedAddresses { get; set; }


        /// <summary>
        /// MachineName metadata where Sender API was triggered.
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// ApplicationName metadata where Sender API was triggered.
        /// </summary>
        public string ApplicationName { get; set; }


        //methods
        public virtual SignalEvent<TKey> CreateClone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object result = bf.Deserialize(ms);
            ms.Close();

            return (SignalEvent<TKey>)result;
        }
    }
}
