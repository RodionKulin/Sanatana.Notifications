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
        public TKey SignalEventId { get; set; }
        public Dictionary<string, string> TemplateData { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public int CategoryId { get; set; }
        public string TopicId { get; set; }
        public int FailedAttempts { get; set; }
        public TKey? EventSettingsId { get; set; }
        public AddresseeType AddresseeType { get; set; }


        //AddresseeType.AllSubscsribers - Fetching subscribers and previous progress fetching subscriber ids.
        public Dictionary<string, string> SubscriberFiltersData { get; set; }
        public TKey? SubscriberIdRangeFrom { get; set; }
        public TKey? SubscriberIdRangeTo { get; set; }
        public List<int> SubscriberIdFromDeliveryTypesHandled { get; set; }


        //AddresseeType.SpecifiedSubscribersById
        public List<TKey> PredefinedSubscriberIds { get; set; }


        //AddresseeType.DirectAddresses
        public List<DeliveryAddress> PredefinedAddresses { get; set; }


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
