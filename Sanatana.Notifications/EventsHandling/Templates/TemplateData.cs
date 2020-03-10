
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.EventsHandling.Templates
{
    public class TemplateData
    {
        //properties
        public Dictionary<string, string> KeyValueModel { get; private set; }
        public object ObjectModel { get; private set; }
        public CultureInfo Culture { get; set; }



        //init
        public TemplateData(Dictionary<string, string> keyValueModel, CultureInfo culture = null)
        {
            KeyValueModel = keyValueModel;
            Culture = culture;
        }

        public TemplateData(object objectModel, CultureInfo culture = null)
        {
            ObjectModel = objectModel;
            Culture = culture;
        }

        public static TemplateData Empty()
        {
            return new TemplateData(new Dictionary<string, string>());
        }



        //convert
        public static implicit operator TemplateData(Dictionary<string, string> keyValueModel)
        {
            return new TemplateData(keyValueModel);
        }
    }
}
