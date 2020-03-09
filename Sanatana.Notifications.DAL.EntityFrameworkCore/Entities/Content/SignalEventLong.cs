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
        //serialized properties
        public string TemplateDataSerialized
        {
            get
            {
                if (TemplateData == null || TemplateData.Count == 0)
                {
                    return null;
                }
                return JsonConvert.SerializeObject(TemplateData);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    TemplateData = null;
                    return;
                }
                TemplateData = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
            }
        }

        public string SubscriberFiltersDataSerialized
        {
            get
            {
                if (SubscriberFiltersData == null || SubscriberFiltersData.Count == 0)
                {
                    return null;
                }
                return JsonConvert.SerializeObject(SubscriberFiltersData);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    SubscriberFiltersData = null;
                    return;
                }
                SubscriberFiltersData = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
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

                return JsonConvert.SerializeObject(SubscriberIdFromDeliveryTypesHandled);
            }
            set
            {
                if(value == null)
                {
                    SubscriberIdFromDeliveryTypesHandled = null;
                    return;
                }

                SubscriberIdFromDeliveryTypesHandled = JsonConvert.DeserializeObject<List<int>>(value);
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
                    PredefinedSubscriberIds = null;
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
                    PredefinedAddresses = null;
                    return;
                }
                PredefinedAddresses = JsonConvert.DeserializeObject<List<DeliveryAddress>>(value);
            }
        }

    }
}
