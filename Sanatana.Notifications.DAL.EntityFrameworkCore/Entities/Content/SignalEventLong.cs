using Newtonsoft.Json;
using Sanatana.Notifications.DAL;
using Sanatana.Notifications.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore
{
    public class SignalEventLong : SignalEvent<long>
    {
        //serialization properties
        public string DataKeyValuesSerialized
        {
            get
            {
                if(DataKeyValues == null || DataKeyValues.Count == 0)
                {
                    return null;
                }

                var xList = DataKeyValues.Select(
                    x => new XElement("item"
                    , new XAttribute("key", x.Key)
                    , new XAttribute("value", x.Value ?? "")));
                var xElem = new XElement("items", xList);
                return xElem.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    DataKeyValues = new Dictionary<string, string>();
                    return;
                }

                var xElem = XElement.Parse(value);
                DataKeyValues = xElem.Descendants("item").ToDictionary(
                    x => (string)x.Attribute("key"),
                    x => (string)x.Attribute("value"));
            }
        }

        public string SubscriberIdFromDeliveryTypesHandledSerialized
        {
            get
            {
                if (SubscriberIdFromDeliveryTypesHandled == null)
                {
                    return null;
                }

                var xElem = new XElement("items", SubscriberIdFromDeliveryTypesHandled.Select(
                    x => new XElement("item", x)
                 ));
                return xElem.ToString();
            }
            set
            {
                if(value == null)
                {
                    SubscriberIdFromDeliveryTypesHandled = new List<int>();
                    return;
                }

                var xElem = XElement.Parse(value);
                SubscriberIdFromDeliveryTypesHandled = xElem.Descendants("item")
                    .Select(x => int.Parse(x.Value))
                    .ToList();
            }
        }

        public string PredefinedSubscriberIdsSerialized
        {
            get
            {
                if (PredefinedSubscriberIds == null)
                {
                    return null;
                }
                return JsonConvert.SerializeObject(PredefinedSubscriberIds);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                PredefinedSubscriberIds = JsonConvert.DeserializeObject<List<long>>(value);
            }
        }

        public string PredefinedAddressesSerialized
        {
            get
            {
                if (PredefinedAddresses == null)
                {
                    return null;
                }
                return JsonConvert.SerializeObject(PredefinedAddresses);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                PredefinedAddresses = JsonConvert.DeserializeObject<List<DeliveryAddress>>(value);
            }
        }

    }
}
